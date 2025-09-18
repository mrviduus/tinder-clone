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

const Matches = () => {
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
      <TouchableOpacity onPress={() => handleMatchPress(item)} style={{ flex: 0.5 }}>
        <CardItem
          image={mainPhoto ? { uri: `data:image/jpeg;base64,${mainPhoto.photoData}` } : null}
          name={item.displayName}
          isOnline={!!item.lastMessageAt}
          hasVariant
        />
        {item.lastMessage && (
          <View style={{ position: 'absolute', bottom: 10, left: 10, right: 10 }}>
            <Text style={{ color: WHITE, fontSize: 12, backgroundColor: 'rgba(0,0,0,0.5)', padding: 5, borderRadius: 5 }} numberOfLines={2}>
              {item.lastMessage}
            </Text>
          </View>
        )}
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
          <Text style={{ color: WHITE, marginTop: 10 }}>Loading matches...</Text>
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
          <Text style={styles.title}>Matches ({matches.length})</Text>
          <TouchableOpacity onPress={onRefresh}>
            <Icon name="refresh" color={DARK_GRAY} size={20} />
          </TouchableOpacity>
        </View>

        {matches.length === 0 ? (
          <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center' }}>
            <Text style={{ color: WHITE, fontSize: 18, textAlign: 'center' }}>
              No matches yet.{'\n'}Start swiping to find your perfect match!
            </Text>
          </View>
        ) : (
          <FlatList
            numColumns={2}
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

export default Matches;
