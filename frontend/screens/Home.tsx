import React, { useState, useEffect } from "react";
import { View, ImageBackground, ActivityIndicator, Alert, Text } from "react-native";
import { useNavigation } from "@react-navigation/native";
import Swiper from "react-native-deck-swiper";
import { City, Filters, CardItem } from "../components";
import { FeedService } from "../src/services/feedService";
import { SwipeService } from "../src/services/swipeService";
import { MatchService } from "../src/services/matchService";
import { Profile, SwipeDirection } from "../src/types/api";
import styles, { PRIMARY_COLOR, WHITE } from "../assets/styles";

const Home = () => {
  const navigation = useNavigation();
  const [swiper, setSwiper] = useState<any>(null);
  const [candidates, setCandidates] = useState<Profile[]>([]);
  const [loading, setLoading] = useState(true);
  const [currentIndex, setCurrentIndex] = useState(0);

  useEffect(() => {
    loadCandidates();
  }, []);

  const loadCandidates = async () => {
    try {
      setLoading(true);
      console.log('Loading candidates...');
      const profiles = await FeedService.getCandidates(50, 1, 20);
      console.log('Candidates loaded:', profiles.length);
      profiles.forEach((profile, index) => {
        console.log(`Candidate ${index}:`, profile);
        console.log(`Photos for ${profile.displayName}:`, profile.photos);
        if (profile.photos && profile.photos.length > 0) {
          console.log('First photo:', profile.photos[0]);
          console.log('Photo data length:', profile.photos[0].photoData?.length || 0);
        }
      });
      setCandidates(profiles);
    } catch (error) {
      console.error('Failed to load candidates:', error);
      Alert.alert('Error', `Failed to load candidates: ${error}`);
    } finally {
      setLoading(false);
    }
  };

  const handleSwipe = async (direction: SwipeDirection, profile: Profile) => {
    try {
      // Convert enum to string for backend API
      const directionString = direction === SwipeDirection.Like ? 'like' : 'pass';

      const result = await SwipeService.processSwipe({
        targetUserId: profile.userId,
        direction: directionString,
      });

      if (result.isMatch) {
        // Fetch match details to get the proper match ID and user info
        const matches = await MatchService.getMatches();
        const newMatch = matches.find(m =>
          m.otherUserId === profile.userId ||
          m.matchId === result.matchId
        );

        Alert.alert(
          '🎉 It\'s a Match!',
          `You and ${profile.displayName} liked each other!`,
          [
            {
              text: 'Keep Swiping',
              style: 'cancel'
            },
            {
              text: 'Send Message',
              onPress: () => {
                // Navigate to Messages with match info
                navigation.navigate('Messages' as never, {
                  matchId: result.matchId || newMatch?.matchId,
                  matchName: profile.displayName
                } as never);
              }
            }
          ]
        );
      }

      // Load more candidates if running low
      if (currentIndex >= candidates.length - 3) {
        const newCandidates = await FeedService.getCandidates(50, Math.floor(candidates.length / 20) + 1, 20);
        setCandidates(prev => [...prev, ...newCandidates]);
      }
    } catch (error: any) {
      console.error('Swipe failed:', error);

      // Provide more specific error messages
      if (error.response?.status === 401) {
        Alert.alert('Session Expired', 'Please login again.');
      } else if (error.response?.status === 429) {
        Alert.alert('Slow Down', 'You\'re swiping too fast. Take a break!');
      } else if (error.message?.includes('Network')) {
        Alert.alert('Connection Issue', 'Check your internet connection and try again.');

        // Queue for offline processing
        await SwipeService.queueSwipe({
          targetUserId: profile.userId,
          direction: directionString,
        });
      } else {
        Alert.alert('Error', 'Failed to process swipe. Please try again.');
      }
    }
  };

  const onSwipedLeft = (cardIndex: number) => {
    const profile = candidates[cardIndex];
    if (profile) {
      handleSwipe(SwipeDirection.Pass, profile);
    }
    setCurrentIndex(cardIndex + 1);
  };

  const onSwipedRight = (cardIndex: number) => {
    const profile = candidates[cardIndex];
    if (profile) {
      handleSwipe(SwipeDirection.Like, profile);
    }
    setCurrentIndex(cardIndex + 1);
  };

  if (loading && candidates.length === 0) {
    return (
      <ImageBackground
        source={require("../assets/images/bg.png")}
        style={styles.bg}
      >
        <View style={[styles.containerHome, { justifyContent: 'center', alignItems: 'center' }]}>
          <ActivityIndicator size="large" color={PRIMARY_COLOR} />
          <Text style={{ color: WHITE, marginTop: 10 }}>Loading candidates...</Text>
        </View>
      </ImageBackground>
    );
  }

  if (candidates.length === 0) {
    return (
      <ImageBackground
        source={require("../assets/images/bg.png")}
        style={styles.bg}
      >
        <View style={[styles.containerHome, { justifyContent: 'center', alignItems: 'center' }]}>
          <Text style={{ color: WHITE, fontSize: 18, textAlign: 'center' }}>
            No more candidates in your area.{'\n'}Try expanding your search radius!
          </Text>
        </View>
      </ImageBackground>
    );
  }

  return (
    <ImageBackground
      source={require("../assets/images/bg.png")}
      style={styles.bg}
    >
      <View style={styles.containerHome}>
        <View style={styles.top}>
          <City />
          <Filters />
        </View>

        <Swiper
          cards={candidates}
          renderCard={(item: Profile) => {
            const mainPhoto = item.photos?.find(p => p.isMain) || item.photos?.[0];
            const age = new Date().getFullYear() - new Date(item.birthDate).getFullYear();

            console.log('Rendering card for:', item.displayName);
            console.log('Main photo:', mainPhoto);
            console.log('Photo data available:', !!mainPhoto?.photoData);
            console.log('Photo data length:', mainPhoto?.photoData?.length || 0);

            const imageSource = mainPhoto ? { uri: `data:image/jpeg;base64,${mainPhoto.photoData}` } : null;
            console.log('Image source:', imageSource);

            return (
              <CardItem
                hasActions
                image={imageSource}
                name={`${item.displayName}, ${age}`}
                description={item.bio || `${item.jobTitle || 'Professional'}${item.company ? ` at ${item.company}` : ''}`}
                matches={item.photos?.length || 0}
              />
            );
          }}
          infinite={false}
          verticalSwipe={false}
          onSwipedLeft={onSwipedLeft}
          onSwipedRight={onSwipedRight}
          ref={(newSwiper: any): void => setSwiper(newSwiper)}
          backgroundColor="transparent"
          stackSize={3}
          cardIndex={0}
          animateCardOpacity
        />
      </View>
    </ImageBackground>
  );
};

export default Home;
