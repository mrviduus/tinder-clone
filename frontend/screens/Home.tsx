import React, { useState, useEffect } from "react";
import { View, ImageBackground, ActivityIndicator, Alert, Text } from "react-native";
import Swiper from "react-native-deck-swiper";
import { City, Filters, CardItem } from "../components";
import { FeedService } from "../src/services/feedService";
import { SwipeService } from "../src/services/swipeService";
import { Profile, SwipeDirection } from "../src/types/api";
import styles, { PRIMARY_COLOR, WHITE } from "../assets/styles";

const Home = () => {
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
      const profiles = await FeedService.getCandidates(50, 1, 20);
      setCandidates(profiles);
    } catch (error) {
      Alert.alert('Error', 'Failed to load candidates');
    } finally {
      setLoading(false);
    }
  };

  const handleSwipe = async (direction: SwipeDirection, profile: Profile) => {
    try {
      const result = await SwipeService.processSwipe({
        targetUserId: profile.userId,
        direction,
      });

      if (result.isMatch) {
        Alert.alert('🎉 It\'s a Match!', `You and ${profile.displayName} liked each other!`);
      }

      // Load more candidates if running low
      if (currentIndex >= candidates.length - 3) {
        const newCandidates = await FeedService.getCandidates(50, Math.floor(candidates.length / 20) + 1, 20);
        setCandidates(prev => [...prev, ...newCandidates]);
      }
    } catch (error) {
      Alert.alert('Error', 'Failed to process swipe');
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

            return (
              <CardItem
                hasActions
                image={mainPhoto ? { uri: `data:image/jpeg;base64,${mainPhoto.photoData}` } : null}
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
