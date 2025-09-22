import React, { useState, useEffect } from "react";
import {
  ScrollView,
  View,
  Text,
  TouchableOpacity,
  ImageBackground,
  FlatList,
  ActivityIndicator,
  Alert,
  RefreshControl,
} from "react-native";
import { useNavigation } from "@react-navigation/native";
import { CardItem, Icon } from "../components";
import { MatchService } from "../src/services/matchService";
import { Match } from "../src/types/api";
import styles, { DARK_GRAY, PRIMARY_COLOR, WHITE } from "../assets/styles";

const Chat = () => {
  const [matches, setMatches] = useState<Match[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const navigation = useNavigation();

  useEffect(() => {
    loadMatches();
  }, []);

  const loadMatches = async () => {
    try {
      setLoading(true);
      const matchesData = await MatchService.getMatches();
      setMatches(matchesData);
    } catch (error) {
      Alert.alert('Error', 'Failed to load matches');
    } finally {
      setLoading(false);
    }
  };

  const onRefresh = async () => {
    setRefreshing(true);
    await loadMatches();
    setRefreshing(false);
  };

  const handleMatchPress = (match: Match) => {
    navigation.navigate('Messages' as never, { matchId: match.matchId, matchName: match.displayName } as never);
  };

  const renderMatch = ({ item }: { item: Match }) => {
    const mainPhoto = item.photos?.find(p => p.isMain) || item.photos?.[0];

    return (
      <TouchableOpacity onPress={() => handleMatchPress(item)} style={{ paddingHorizontal: 16, paddingVertical: 8 }}>
        <View style={{ flexDirection: 'row', alignItems: 'center', backgroundColor: 'rgba(255,255,255,0.1)', borderRadius: 12, padding: 12 }}>
          <View style={{ width: 60, height: 60, borderRadius: 30, overflow: 'hidden', marginRight: 12 }}>
            {mainPhoto ? (
              <CardItem
                image={{ uri: `data:image/jpeg;base64,${mainPhoto.photoData}` }}
                name=""
                hasVariant
              />
            ) : (
              <View style={{ width: 60, height: 60, backgroundColor: DARK_GRAY, borderRadius: 30 }} />
            )}
          </View>
          <View style={{ flex: 1 }}>
            <Text style={{ color: WHITE, fontSize: 18, fontWeight: 'bold' }}>{item.displayName}</Text>
            {item.lastMessage && (
              <Text style={{ color: DARK_GRAY, fontSize: 14, marginTop: 4 }} numberOfLines={1}>
                {item.lastMessage}
              </Text>
            )}
            {item.lastMessageAt && (
              <Text style={{ color: DARK_GRAY, fontSize: 12, marginTop: 2 }}>
                {new Date(item.lastMessageAt).toLocaleDateString()}
              </Text>
            )}
          </View>
          <Icon name="chevron-forward" color={DARK_GRAY} size={20} />
        </View>
      </TouchableOpacity>
    );
  };

  if (loading) {
    return (
      <ImageBackground
        source={require("../assets/images/bg.png")}
        style={styles.bg}
      >
        <View style={[styles.containerMatches, { justifyContent: 'center', alignItems: 'center' }]}>
          <ActivityIndicator size="large" color={PRIMARY_COLOR} />
          <Text style={{ color: WHITE, marginTop: 10 }}>Loading chats...</Text>
        </View>
      </ImageBackground>
    );
  }

  return (
    <ImageBackground
      source={require("../assets/images/bg.png")}
      style={styles.bg}
    >
      <View style={styles.containerMatches}>
        <View style={styles.top}>
          <Text style={styles.title}>Messages ({matches.length})</Text>
          <TouchableOpacity onPress={onRefresh}>
            <Icon name="refresh" color={DARK_GRAY} size={20} />
          </TouchableOpacity>
        </View>

        {matches.length === 0 ? (
          <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center' }}>
            <Text style={{ color: WHITE, fontSize: 18, textAlign: 'center' }}>
              No matches yet.{'\n'}Start swiping to find your perfect match and start chatting!
            </Text>
          </View>
        ) : (
          <FlatList
            data={matches}
            keyExtractor={(item) => item.matchId}
            renderItem={renderMatch}
            refreshControl={
              <RefreshControl
                refreshing={refreshing}
                onRefresh={onRefresh}
                tintColor={WHITE}
              />
            }
            contentContainerStyle={{ paddingBottom: 20 }}
          />
        )}
      </View>
    </ImageBackground>
  );
};

export default Chat;