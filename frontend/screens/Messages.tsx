import React, { useState, useEffect, useRef } from "react";
import {
  ScrollView,
  Text,
  TouchableOpacity,
  ImageBackground,
  View,
  FlatList,
  TextInput,
  KeyboardAvoidingView,
  Platform,
  ActivityIndicator,
  Alert,
} from "react-native";
import { useRoute, useNavigation } from "@react-navigation/native";
import { Icon, Message } from "../components";
import { MessageService } from "../src/services/messageService";
import { MatchService } from "../src/services/matchService";
import { signalRService } from "../src/services/signalrService";
import { useAuthStore } from "../src/store/authStore";
import { Message as MessageType } from "../src/types/api";
import styles, { DARK_GRAY, PRIMARY_COLOR, WHITE } from "../assets/styles";

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
    };
  }, [matchId]);

  const loadMessages = async () => {
    if (!matchId) return;

    try {
      setLoading(true);
      const messagesData = await MessageService.getMessages(matchId);
      setMessages(messagesData.reverse()); // Reverse to show latest at bottom
    } catch (error) {
      Alert.alert('Error', 'Failed to load messages');
    } finally {
      setLoading(false);
    }
  };

  const setupSignalR = async () => {
    if (!matchId) return;

    try {
      await signalRService.connect();
      await signalRService.joinMatch(matchId);

      const unsubscribeMessage = signalRService.onMessage((message: MessageType) => {
        setMessages(prev => [...prev, message]);
        setTimeout(() => flatListRef.current?.scrollToEnd(), 100);
      });

      const unsubscribeTyping = signalRService.onTyping((data: any) => {
        if (data.userId !== user?.id) {
          setIsTyping(data.isTyping !== false);
        }
      });

      return () => {
        unsubscribeMessage();
        unsubscribeTyping();
      };
    } catch (error) {
      console.error('SignalR setup error:', error);
    }
  };

  const sendMessage = async () => {
    if (!newMessage.trim() || !matchId || sending) return;

    const messageText = newMessage.trim();
    setNewMessage('');
    setSending(true);

    try {
      await MessageService.sendMessage({
        matchId,
        content: messageText,
      });

      // Stop typing indicator
      await signalRService.stopTyping(matchId);
    } catch (error) {
      Alert.alert('Error', 'Failed to send message');
      setNewMessage(messageText); // Restore message on error
    } finally {
      setSending(false);
    }
  };

  const handleTyping = async (text: string) => {
    setNewMessage(text);

    if (!matchId) return;

    if (text.length > 0) {
      await signalRService.sendTyping(matchId);

      // Clear existing timeout
      if (typingTimeoutRef.current) {
        clearTimeout(typingTimeoutRef.current);
      }

      // Set new timeout to stop typing after 2 seconds
      typingTimeoutRef.current = setTimeout(async () => {
        await signalRService.stopTyping(matchId);
      }, 2000);
    } else {
      await signalRService.stopTyping(matchId);
      if (typingTimeoutRef.current) {
        clearTimeout(typingTimeoutRef.current);
      }
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
      <View style={{
        alignItems: isMyMessage ? 'flex-end' : 'flex-start',
        marginVertical: 4,
        marginHorizontal: 16,
      }}>
        <View style={{
          backgroundColor: isMyMessage ? PRIMARY_COLOR : '#f0f0f0',
          padding: 12,
          borderRadius: 16,
          maxWidth: '80%',
        }}>
          <Text style={{
            color: isMyMessage ? WHITE : '#000',
            fontSize: 16,
          }}>
            {item.content}
          </Text>
          <Text style={{
            color: isMyMessage ? WHITE : DARK_GRAY,
            fontSize: 12,
            marginTop: 4,
            opacity: 0.7,
          }}>
            {new Date(item.sentAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
          </Text>
        </View>
      </View>
    );
  };

  if (loading) {
    return (
      <ImageBackground
        source={require("../assets/images/bg.png")}
        style={styles.bg}
      >
        <View style={[styles.containerMessages, { justifyContent: 'center', alignItems: 'center' }]}>
          <ActivityIndicator size="large" color={PRIMARY_COLOR} />
          <Text style={{ color: WHITE, marginTop: 10 }}>Loading messages...</Text>
        </View>
      </ImageBackground>
    );
  }

  return (
    <ImageBackground
      source={require("../assets/images/bg.png")}
      style={styles.bg}
    >
      <KeyboardAvoidingView
        style={{ flex: 1 }}
        behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
      >
        <View style={styles.containerMessages}>
          <View style={styles.top}>
            <TouchableOpacity onPress={() => navigation.goBack()}>
              <Icon name="chevron-back" color={DARK_GRAY} size={20} />
            </TouchableOpacity>
            <Text style={[styles.title, { flex: 1, textAlign: 'center' }]}>{matchName}</Text>
            <TouchableOpacity onPress={handleUnmatch}>
              <Icon name="close-circle-outline" color={DARK_GRAY} size={24} />
            </TouchableOpacity>
          </View>

          <FlatList
            ref={flatListRef}
            data={messages}
            keyExtractor={(item) => item.messageId}
            renderItem={renderMessage}
            contentContainerStyle={{ paddingVertical: 16 }}
            onContentSizeChange={() => flatListRef.current?.scrollToEnd()}
          />

          {isTyping && (
            <View style={{ padding: 16, alignItems: 'flex-start' }}>
              <View style={{
                backgroundColor: '#f0f0f0',
                padding: 12,
                borderRadius: 16,
              }}>
                <Text style={{ color: DARK_GRAY, fontStyle: 'italic' }}>
                  {matchName} is typing...
                </Text>
              </View>
            </View>
          )}

          <View style={{
            flexDirection: 'row',
            padding: 16,
            alignItems: 'flex-end',
            backgroundColor: WHITE,
          }}>
            <TextInput
              style={{
                flex: 1,
                borderWidth: 1,
                borderColor: DARK_GRAY,
                borderRadius: 20,
                paddingHorizontal: 16,
                paddingVertical: 8,
                marginRight: 8,
                maxHeight: 100,
              }}
              placeholder="Type a message..."
              value={newMessage}
              onChangeText={handleTyping}
              multiline
              onSubmitEditing={sendMessage}
            />
            <TouchableOpacity
              onPress={sendMessage}
              disabled={!newMessage.trim() || sending}
              style={{
                backgroundColor: newMessage.trim() ? PRIMARY_COLOR : DARK_GRAY,
                borderRadius: 20,
                padding: 8,
              }}
            >
              {sending ? (
                <ActivityIndicator size="small" color={WHITE} />
              ) : (
                <Icon name="send" color={WHITE} size={20} />
              )}
            </TouchableOpacity>
          </View>
        </View>
      </KeyboardAvoidingView>
    </ImageBackground>
  );
};

export default Messages;
