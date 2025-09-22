import React, { useEffect, useState } from "react";
import { NavigationContainer } from "@react-navigation/native";
import { createStackNavigator } from "@react-navigation/stack";
import { createBottomTabNavigator } from "@react-navigation/bottom-tabs";
import { Home, Matches, Messages, Chat, Profile } from "./screens";
import LoginScreen from "./src/screens/Login";
import RegisterScreen from "./src/screens/Register";
import { useAuthStore } from "./src/store/authStore";
import { PRIMARY_COLOR, DARK_GRAY, BLACK, WHITE } from "./assets/styles";
import TabBarIcon from "./components/TabBarIcon";
import * as Font from "expo-font";
import * as SplashScreen from "expo-splash-screen";
import { Ionicons } from "@expo/vector-icons";

// Keep the splash screen visible while we fetch resources
SplashScreen.preventAutoHideAsync();

const Stack = createStackNavigator();
const Tab = createBottomTabNavigator();

const MainTabNavigator = () => (
  <Tab.Navigator
    screenOptions={{
      tabBarShowLabel: false,
      tabBarActiveTintColor: PRIMARY_COLOR,
      tabBarInactiveTintColor: DARK_GRAY,
      tabBarLabelStyle: {
        fontSize: 14,
        textTransform: "uppercase",
        paddingTop: 10,
      },
      tabBarStyle: {
        backgroundColor: WHITE,
        borderTopWidth: 0,
        marginBottom: 0,
        shadowOpacity: 0.05,
        shadowRadius: 10,
        shadowColor: BLACK,
        shadowOffset: { height: 0, width: 0 },
      },
    }}
  >
    <Tab.Screen
      name="Explore"
      component={Home}
      options={{
        tabBarIcon: ({ focused }) => (
          <TabBarIcon
            focused={focused}
            iconName="search"
            text="Explore"
          />
        ),
      }}
    />

    <Tab.Screen
      name="Matches"
      component={Matches}
      options={{
        tabBarIcon: ({ focused }) => (
          <TabBarIcon
            focused={focused}
            iconName="heart"
            text="Matches"
          />
        ),
      }}
    />

    <Tab.Screen
      name="Chat"
      component={Chat}
      options={{
        tabBarIcon: ({ focused }) => (
          <TabBarIcon
            focused={focused}
            iconName="chatbubble"
            text="Chat"
          />
        ),
      }}
    />

    <Tab.Screen
      name="Profile"
      component={Profile}
      options={{
        tabBarIcon: ({ focused }) => (
          <TabBarIcon
            focused={focused}
            iconName="person"
            text="Profile"
          />
        ),
      }}
    />
  </Tab.Navigator>
);

const App = () => {
  const [appIsReady, setAppIsReady] = useState(false);
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);

  useEffect(() => {
    async function prepare() {
      try {
        // Pre-load fonts, make any API calls you need to do here
        await Font.loadAsync(Ionicons.font);
      } catch (e) {
        console.warn(e);
      } finally {
        // Tell the application to render
        setAppIsReady(true);
      }
    }

    prepare();
  }, []);

  useEffect(() => {
    if (appIsReady) {
      // Hide the splash screen once the app is ready
      SplashScreen.hideAsync();
    }
  }, [appIsReady]);

  if (!appIsReady) {
    return null;
  }

  return (
    <NavigationContainer>
      <Stack.Navigator screenOptions={{ headerShown: false }}>
        {isAuthenticated ? (
          <>
            <Stack.Screen name="Main" component={MainTabNavigator} />
            <Stack.Screen name="Messages" component={Messages} />
          </>
        ) : (
          <>
            <Stack.Screen name="Login" component={LoginScreen} />
            <Stack.Screen name="Register" component={RegisterScreen} />
          </>
        )}
      </Stack.Navigator>
    </NavigationContainer>
  );
};

export default App;
