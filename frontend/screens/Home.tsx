import React, { useState } from "react";
import { View, ImageBackground } from "react-native";
import Swiper from "react-native-deck-swiper";
import { City, Filters, CardItem } from "../components";
import styles from "../assets/styles";
import DEMO from "../assets/data/demo";
import { DataT } from "../types";

const Home = () => {
  const [swiper, setSwiper] = useState<any>(null);

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
          cards={DEMO}
          renderCard={(item: DataT) => (
            <CardItem
              hasActions
              image={item.image}
              name={item.name}
              description={item.description}
              matches={item.match}
            />
          )}
          infinite
          verticalSwipe={false}
          ref={(newSwiper: any): void => setSwiper(newSwiper)}
          backgroundColor="transparent"
          stackSize={3}
        />
      </View>
    </ImageBackground>
  );
};

export default Home;
