import React, { useState, useEffect, useRef, useLayoutEffect } from "react";
import {
  Text,
  TouchableOpacity,
  View,
  FlatList,
  TextInput,
  KeyboardAvoidingView,
  Platform,
  ActivityIndicator,
  Alert,
  SafeAreaView,
  Keyboard,
  Dimensions,
} from "react-native";
import { useRoute, useNavigation } from "@react-navigation/native";
import { Icon } from "../components";
import { MessageService } from "../src/services/messageService";
import { MatchService } from "../src/services/matchService";
import { signalRService } from "../src/services/signalrService";
import { useAuthStore } from "../src/store/authStore";
import { Message as MessageType } from "../src/types/api";
import styles, { DARK_GRAY, PRIMARY_COLOR, WHITE } from "../assets/styles";

const { height: SCREEN_HEIGHT } = Dimensions.get('window');

const Messages = () => {
  const route = useRoute();
  const navigation = useNavigation();
  const { user } = useAuthStore();
  const [messages, setMessages] = useState<MessageType[]>([]);
  const [newMessage, setNewMessage] = useState('');
  const [loading, setLoading] = useState(true);
  const [sending, setSending] = useState(false);
  const [isTyping, setIsTyping] = useState(false);
  const flatListRef = useRef<FlatList>(null);
  const typingTimeoutRef = useRef<NodeJS.Timeout>();

  // Get route params
  const params = route.params as { matchId?: string; matchName?: string } | undefined;
  const matchId = params?.matchId;
  const matchName = params?.matchName || 'Chat';

  useEffect(() => {
    if (!matchId) {
      navigation.goBack();
      return;
    }

    loadMessages();
    setupSignalR();

    return () => {
      signalRService.leaveMatch(matchId);
      if (typingTimeoutRef.current) {
        clearTimeout(typingTimeoutRef.current);
      }
    };
  }, [matchId]);

  const loadMessages = async () => {
    if (!matchId) return;

    try {
      setLoading(true);
      const messagesData = await MessageService.getMessages(matchId);
      // Messages come from backend already sorted by SentAt ascending
      setMessages(messagesData);
      // Auto-scroll to bottom after loading
      setTimeout(() => {
        if (flatListRef.current && messagesData.length > 0) {
          flatListRef.current.scrollToEnd({ animated: false });
        }
      }, 100);
    } catch (error) {
      Alert.alert('Error', 'Failed to load messages');
    } finally {
      setLoading(false);
    }
  };

  const setupSignalR = async () => {
    if (!matchId) return;

    try {
      // Ensure connection is established
      await signalRService.connect();

      // Join the match group
      await signalRService.joinMatch(matchId);

      // Subscribe to message events
      const unsubscribeMessage = signalRService.onMessage((message: MessageType) => {
        console.log('Received message in component:', message);
        setMessages(prev => {
          // Normalize the message ID field
          const messageId = message.messageId || (message as any).id;
          const exists = prev.some(m => {
            const mId = m.messageId || (m as any).id;
            return mId === messageId;
          });

          if (exists) {
            console.log('Message already exists, skipping');
            return prev;
          }

          console.log('Adding new message to chat');
          return [...prev, message];
        });

        // Auto-scroll to the latest message
        setTimeout(() => {
          flatListRef.current?.scrollToEnd({ animated: true });
        }, 100);
      });

      // Subscribe to typing events
      const unsubscribeTyping = signalRService.onTyping((data: any) => {
        console.log('Typing event received:', data);
        // Check both possible field name formats
        const typingUserId = data.UserId || data.userId;
        const typingMatchId = data.MatchId || data.matchId;
        const isTyping = data.IsTyping !== undefined ? data.IsTyping : data.isTyping;

        if (typingUserId !== user?.id && typingMatchId === matchId) {
          setIsTyping(isTyping === true);

          // Auto-hide typing indicator after 3 seconds
          if (isTyping) {
            setTimeout(() => setIsTyping(false), 3000);
          }
        }
      });

      return () => {
        unsubscribeMessage();
        unsubscribeTyping();
      };
    } catch (error) {
      console.error('SignalR setup error:', error);
      Alert.alert('Connection Error', 'Failed to connect to chat server. Please try again.');
    }
  };

  const sendMessage = async () => {
    if (!newMessage.trim() || !matchId || sending) return;

    const messageText = newMessage.trim();
    setNewMessage('');
    setSending(true);

    // Optimistically add message to UI
    const optimisticMessage: MessageType = {
      messageId: `temp-${Date.now()}`,
      matchId,
      senderId: user?.id || '',
      content: messageText,
      sentAt: new Date().toISOString(),
    };

    setMessages(prev => [...prev, optimisticMessage]);

    // Auto-scroll immediately
    setTimeout(() => {
      flatListRef.current?.scrollToEnd({ animated: true });
    }, 50);

    Keyboard.dismiss();

    try {
      // Send via SignalR - the real message will come back through MessageReceived event
      await signalRService.sendMessage(matchId, messageText);

      // Remove optimistic message when real one arrives (handled in setupSignalR)
      setMessages(prev => prev.filter(m => m.messageId !== optimisticMessage.messageId));

      // Stop typing indicator
      await signalRService.stopTyping(matchId);
    } catch (error) {
      console.error('Failed to send message:', error);

      // Remove optimistic message on failure
      setMessages(prev => prev.filter(m => m.messageId !== optimisticMessage.messageId));

      Alert.alert('Error', 'Failed to send message. Please check your connection.');
      setNewMessage(messageText);
    } finally {
      setSending(false);
    }
  };

  const handleTyping = async (text: string) => {
    setNewMessage(text);

    if (!matchId) return;

    try {
      if (text.length > 0 && !sending) {
        // Send typing indicator
        await signalRService.sendTyping(matchId);

        // Clear existing timeout
        if (typingTimeoutRef.current) {
          clearTimeout(typingTimeoutRef.current);
        }

        // Set timeout to stop typing after 2 seconds of inactivity
        typingTimeoutRef.current = setTimeout(async () => {
          try {
            await signalRService.stopTyping(matchId);
          } catch (error) {
            console.error('Failed to stop typing indicator:', error);
          }
        }, 2000);
      } else if (text.length === 0) {
        // Stop typing when text is cleared
        await signalRService.stopTyping(matchId);
        if (typingTimeoutRef.current) {
          clearTimeout(typingTimeoutRef.current);
        }
      }
    } catch (error) {
      console.error('Failed to send typing indicator:', error);
    }
  };

  const handleUnmatch = async () => {
    if (!matchId) return;

    Alert.alert(
      'Unmatch',
      `Are you sure you want to unmatch with ${matchName}? This action cannot be undone.`,
      [
        { text: 'Cancel', style: 'cancel' },
        {
          text: 'Unmatch',
          style: 'destructive',
          onPress: async () => {
            const success = await MatchService.unmatch(matchId);
            if (success) {
              navigation.goBack();
            } else {
              Alert.alert('Error', 'Failed to unmatch');
            }
          },
        },
      ],
    );
  };

  const renderMessage = ({ item }: { item: MessageType }) => {
    const isMyMessage = item.senderId === user?.id;

    return (
      <View
        style={{
          alignItems: isMyMessage ? 'flex-end' : 'flex-start',
          marginVertical: 6,
          marginHorizontal: 16,
        }}
      >
        <View style={{
          backgroundColor: isMyMessage ? PRIMARY_COLOR : '#f0f0f0',
          padding: 12,
          borderRadius: 18,
          maxWidth: '75%',
          minWidth: 80,
        }}>
          <Text style={{
            color: isMyMessage ? WHITE : '#000',
            fontSize: 16,
            lineHeight: 20,
          }}>
            {item.content}
          </Text>
          <Text style={{
            color: isMyMessage ? 'rgba(255,255,255,0.7)' : '#999',
            fontSize: 11,
            marginTop: 4,
          }}>
            {new Date(item.sentAt).toLocaleTimeString([], {
              hour: '2-digit',
              minute: '2-digit'
            })}
          </Text>
        </View>
      </View>
    );
  };

  if (loading) {
    return (
      <SafeAreaView style={{ flex: 1, backgroundColor: WHITE }}>
        <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center' }}>
          <ActivityIndicator size="large" color={PRIMARY_COLOR} />
          <Text style={{ color: DARK_GRAY, marginTop: 10 }}>Loading messages...</Text>
        </View>
      </SafeAreaView>
    );
  }

  // Calculate heights for proper layout
  const HEADER_HEIGHT = 60;
  const INPUT_CONTAINER_HEIGHT = 80;
  const SAFE_AREA_BOTTOM = Platform.OS === 'ios' ? 34 : 0;
  const MESSAGE_CONTAINER_HEIGHT = SCREEN_HEIGHT - HEADER_HEIGHT - INPUT_CONTAINER_HEIGHT - SAFE_AREA_BOTTOM - 100;

  return (
    <SafeAreaView style={{ flex: 1, backgroundColor: WHITE }}>
      <KeyboardAvoidingView
        style={{ flex: 1 }}
        behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
        keyboardVerticalOffset={0}
      >
        <View style={{ flex: 1, backgroundColor: WHITE }}>
          {/* Fixed Header */}
          <View style={{
            height: HEADER_HEIGHT,
            flexDirection: 'row',
            alignItems: 'center',
            paddingHorizontal: 16,
            borderBottomWidth: 1,
            borderBottomColor: '#e0e0e0',
            backgroundColor: WHITE,
          }}>
            <TouchableOpacity
              onPress={() => navigation.goBack()}
              style={{ padding: 8 }}
            >
              <Icon name="chevron-back" color={DARK_GRAY} size={24} />
            </TouchableOpacity>
            <Text style={{
              flex: 1,
              fontSize: 18,
              fontWeight: '600',
              textAlign: 'center',
              color: '#000',
            }}>
              {matchName}
            </Text>
            <TouchableOpacity
              onPress={handleUnmatch}
              testID="unmatch-button"
              style={{ padding: 8 }}
            >
              <Icon name="close-circle-outline" color={DARK_GRAY} size={24} />
            </TouchableOpacity>
          </View>

          {/* Scrollable Messages Container with Fixed Height */}
          <View style={{
            flex: 1,
            maxHeight: MESSAGE_CONTAINER_HEIGHT,
            backgroundColor: '#f8f8f8',
          }}>
            <FlatList
              ref={flatListRef}
              data={messages}
              keyExtractor={(item) => item.messageId || String(Math.random())}
              renderItem={renderMessage}
              contentContainerStyle={{
                paddingVertical: 16,
                flexGrow: 1,
                justifyContent: messages.length > 0 ? 'flex-start' : 'center',
              }}
              showsVerticalScrollIndicator={true}
              maintainVisibleContentPosition={{
                minIndexForVisible: 0,
                autoscrollToTopThreshold: 10,
              }}
              onContentSizeChange={() => {
                if (messages.length > 0) {
                  flatListRef.current?.scrollToEnd({ animated: false });
                }
              }}
              onLayout={() => {
                if (messages.length > 0) {
                  flatListRef.current?.scrollToEnd({ animated: false });
                }
              }}
              ListEmptyComponent={
                <View style={{
                  flex: 1,
                  justifyContent: 'center',
                  alignItems: 'center',
                  padding: 40,
                }}>
                  <Text style={{
                    color: DARK_GRAY,
                    fontSize: 16,
                    textAlign: 'center'
                  }}>
                    Start a conversation with {matchName}
                  </Text>
                </View>
              }
              // Enable scroll indicator
              indicatorStyle="black"
              scrollIndicatorInsets={{ right: 1 }}
            />

            {/* Typing Indicator */}
            {isTyping && (
              <View style={{
                position: 'absolute',
                bottom: 8,
                left: 16,
                backgroundColor: '#f0f0f0',
                padding: 12,
                borderRadius: 18,
                flexDirection: 'row',
                alignItems: 'center',
              }}>
                <Text style={{ color: DARK_GRAY, fontSize: 14 }}>
                  {matchName} is typing
                </Text>
                <View style={{ flexDirection: 'row', marginLeft: 4 }}>
                  <Text style={{ color: DARK_GRAY }}>...</Text>
                </View>
              </View>
            )}
          </View>

          {/* Fixed Input Container at Bottom */}
          <View style={{
            height: INPUT_CONTAINER_HEIGHT,
            backgroundColor: WHITE,
            borderTopWidth: 1,
            borderTopColor: '#e0e0e0',
            paddingHorizontal: 16,
            paddingVertical: 12,
            flexDirection: 'row',
            alignItems: 'center',
          }}>
            <View style={{
              flex: 1,
              minHeight: 44,
              maxHeight: 100,
              marginRight: 12,
              borderWidth: 1,
              borderColor: '#e0e0e0',
              borderRadius: 22,
              backgroundColor: '#f9f9f9',
              paddingHorizontal: 16,
              justifyContent: 'center',
            }}>
              <TextInput
                style={{
                  fontSize: 16,
                  color: '#000',
                  paddingVertical: Platform.OS === 'ios' ? 10 : 8,
                  maxHeight: 80,
                }}
                placeholder="Type a message..."
                placeholderTextColor="#999"
                value={newMessage}
                onChangeText={handleTyping}
                multiline
                textAlignVertical="center"
                returnKeyType="send"
                blurOnSubmit={false}
                onSubmitEditing={(e) => {
                  // Send message on Enter key (without Shift)
                  if (newMessage.trim() && !e.nativeEvent.text.includes('\n')) {
                    sendMessage();
                  }
                }}
                onKeyPress={(e) => {
                  // Web-specific: Handle Enter key
                  if (Platform.OS === 'web' && e.nativeEvent.key === 'Enter' && !e.nativeEvent.shiftKey) {
                    e.preventDefault();
                    if (newMessage.trim()) {
                      sendMessage();
                    }
                  }
                }}
              />
            </View>

            <TouchableOpacity
              onPress={sendMessage}
              disabled={!newMessage.trim() || sending}
              testID="send-button"
              style={{
                width: 44,
                height: 44,
                borderRadius: 22,
                backgroundColor: newMessage.trim() && !sending ? PRIMARY_COLOR : '#e0e0e0',
                justifyContent: 'center',
                alignItems: 'center',
              }}
            >
              {sending ? (
                <ActivityIndicator size="small" color={WHITE} />
              ) : (
                <Icon
                  name="send"
                  color={newMessage.trim() ? WHITE : '#999'}
                  size={22}
                />
              )}
            </TouchableOpacity>
          </View>
        </View>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
};

export default Messages;