import React from 'react';
import { NavigationContainer } from '@react-navigation/native';
import { createStackNavigator } from '@react-navigation/stack';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import { Ionicons } from '@expo/vector-icons';
import { RootStackParamList, MainTabParamList } from '../types';

// Import screens (these will be created in Phase 3)
// import LoginScreen from '../screens/auth/LoginScreen';
// import RegisterScreen from '../screens/auth/RegisterScreen';
// import FeedScreen from '../screens/feed/FeedScreen';
// import MatchesScreen from '../screens/matches/MatchesScreen';
// import ChatListScreen from '../screens/chat/ChatListScreen';
// import ChatScreen from '../screens/chat/ChatScreen';
// import ProfileScreen from '../screens/profile/ProfileScreen';
// import EditProfileScreen from '../screens/profile/EditProfileScreen';

// Temporary placeholder screens
const PlaceholderScreen = ({ route }: any) => {
  const React = require('react');
  const { View, Text, StyleSheet } = require('react-native');

  return (
    <View style={styles.container}>
      <Text style={styles.text}>{route.name} Screen</Text>
      <Text style={styles.subtext}>Coming Soon</Text>
    </View>
  );
};

const styles = {
  container: {
    flex: 1,
    justifyContent: 'center' as const,
    alignItems: 'center' as const,
    backgroundColor: '#f5f5f5',
  },
  text: {
    fontSize: 24,
    fontWeight: 'bold' as const,
    marginBottom: 10,
  },
  subtext: {
    fontSize: 16,
    color: '#666',
  },
};

const Stack = createStackNavigator<RootStackParamList>();
const Tab = createBottomTabNavigator<MainTabParamList>();

// Main tab navigator for authenticated users
function MainTabNavigator() {
  return (
    <Tab.Navigator
      screenOptions={({ route }) => ({
        tabBarIcon: ({ focused, color, size }) => {
          let iconName: keyof typeof Ionicons.glyphMap;

          switch (route.name) {
            case 'Feed':
              iconName = focused ? 'flame' : 'flame-outline';
              break;
            case 'Matches':
              iconName = focused ? 'heart' : 'heart-outline';
              break;
            case 'Messages':
              iconName = focused ? 'chatbubbles' : 'chatbubbles-outline';
              break;
            case 'Profile':
              iconName = focused ? 'person' : 'person-outline';
              break;
            default:
              iconName = 'help-outline';
          }

          return <Ionicons name={iconName} size={size} color={color} />;
        },
        tabBarActiveTintColor: '#FF4458',
        tabBarInactiveTintColor: 'gray',
        headerShown: true,
        headerStyle: {
          backgroundColor: '#fff',
          elevation: 1,
          shadowOpacity: 0.1,
        },
        headerTitleStyle: {
          fontSize: 20,
          fontWeight: '600',
        },
      })}
    >
      <Tab.Screen
        name="Feed"
        component={PlaceholderScreen}
        options={{
          title: 'Discover',
          headerTitle: 'Discover',
        }}
      />
      <Tab.Screen
        name="Matches"
        component={PlaceholderScreen}
        options={{
          title: 'Matches',
          headerTitle: 'Your Matches',
        }}
      />
      <Tab.Screen
        name="Messages"
        component={PlaceholderScreen}
        options={{
          title: 'Messages',
          headerTitle: 'Messages',
        }}
      />
      <Tab.Screen
        name="Profile"
        component={PlaceholderScreen}
        options={{
          title: 'Profile',
          headerTitle: 'Your Profile',
        }}
      />
    </Tab.Navigator>
  );
}

// Auth stack for unauthenticated users
function AuthStack() {
  return (
    <Stack.Navigator
      screenOptions={{
        headerShown: false,
        cardStyle: { backgroundColor: '#fff' },
      }}
    >
      <Stack.Screen name="Login" component={PlaceholderScreen} />
      <Stack.Screen name="Register" component={PlaceholderScreen} />
    </Stack.Navigator>
  );
}

// Root navigator
export default function AppNavigator() {
  // This will be connected to auth state in Phase 3
  const isAuthenticated = false; // Placeholder

  return (
    <NavigationContainer>
      <Stack.Navigator screenOptions={{ headerShown: false }}>
        {isAuthenticated ? (
          <Stack.Screen name="Main" component={MainTabNavigator} />
        ) : (
          <Stack.Screen name="Auth" component={AuthStack} />
        )}
        {/* Additional screens that can be accessed from anywhere */}
        <Stack.Screen
          name="Chat"
          component={PlaceholderScreen}
          options={{
            headerShown: true,
            headerTitle: 'Chat',
            headerBackTitle: 'Back',
          }}
        />
        <Stack.Screen
          name="EditProfile"
          component={PlaceholderScreen}
          options={{
            headerShown: true,
            headerTitle: 'Edit Profile',
            headerBackTitle: 'Back',
            presentation: 'modal',
          }}
        />
      </Stack.Navigator>
    </NavigationContainer>
  );
}