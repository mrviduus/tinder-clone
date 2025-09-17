# Tinder Clone

A React Native dating app clone built with Expo, featuring swipe gestures, matching functionality, and messaging interface.

![Tinder Clone Preview](frontend/preview/tinderclone-preview.gif)

## Features

- 🔥 Swipe cards interface
- 💕 Matching system
- 💬 Messaging functionality
- 📱 Cross-platform (iOS, Android, Web)
- 🎨 Modern UI design

## Tech Stack

- **React Native** - Cross-platform mobile development
- **Expo** - Development platform and build tools
- **TypeScript** - Type-safe JavaScript
- **React Navigation** - Navigation library
- **React Native Deck Swiper** - Card swiping functionality

## Prerequisites

Before running this project, make sure you have the following installed:

- [Node.js](https://nodejs.org/) (v16 or higher)
- [npm](https://www.npmjs.com/) or [yarn](https://yarnpkg.com/)
- [Expo CLI](https://docs.expo.dev/get-started/installation/)

### For mobile development:
- **iOS**: Xcode (macOS only)
- **Android**: Android Studio and Android SDK

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/mrviduus/tinder-clone.git
   cd tinder-clone
   ```

2. Navigate to the frontend directory:
   ```bash
   cd frontend
   ```

3. Install dependencies:
   ```bash
   npm install
   # or
   yarn install
   ```

## Running the Project

### Development Server

Start the Expo development server:

```bash
npm start
# or
yarn start
```

This will open the Expo Developer Tools in your browser.

### Platform-Specific Commands

#### iOS Simulator
```bash
npm run ios
# or
yarn ios
```

#### Android Emulator
```bash
npm run android
# or
yarn android
```

#### Web Browser
```bash
npm run web
# or
yarn web
```

## Development Setup

### Using Expo Go App (Recommended for beginners)

1. Install the [Expo Go](https://expo.dev/client) app on your mobile device
2. Run `npm start` in the project directory
3. Scan the QR code with your device camera (iOS) or the Expo Go app (Android)

### Using Physical Device

1. Install Expo CLI globally:
   ```bash
   npm install -g expo-cli
   ```

2. Start the development server:
   ```bash
   expo start
   ```

3. Use the Expo Go app to scan the QR code

### Using Simulators/Emulators

#### iOS Simulator (macOS only)
1. Install Xcode from the App Store
2. Run `npm run ios` or press `i` in the Expo CLI

#### Android Emulator
1. Install Android Studio
2. Set up an Android Virtual Device (AVD)
3. Run `npm run android` or press `a` in the Expo CLI

## Project Structure

```
frontend/
├── assets/          # Images, fonts, and other static assets
├── components/      # Reusable React components
├── screens/         # Screen components
├── App.tsx         # Main app component
├── package.json    # Dependencies and scripts
└── tsconfig.json   # TypeScript configuration
```

## Available Scripts

- `npm start` - Start the Expo development server
- `npm run ios` - Run on iOS simulator
- `npm run android` - Run on Android emulator
- `npm run web` - Run in web browser

## Troubleshooting

### Common Issues

1. **Metro bundler issues**: Clear cache with `expo start -c`
2. **Node modules issues**: Delete `node_modules` and run `npm install` again
3. **iOS build issues**: Make sure Xcode is updated to the latest version
4. **Android build issues**: Ensure Android SDK is properly configured

### Reset Project

If you encounter persistent issues:

```bash
# Clear Expo cache
expo start -c

# Clear npm cache
npm cache clean --force

# Reinstall dependencies
rm -rf node_modules
npm install
```

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Built with [Expo](https://expo.dev/)
- UI inspiration from Tinder
- React Native community for amazing libraries