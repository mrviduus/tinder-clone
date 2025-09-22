# iOS Setup Guide - Tinder Clone

## 📱 Prerequisites for iOS Development

### Required Software
1. **macOS** - iOS development requires a Mac
2. **Xcode** (14.0 or later)
   ```bash
   # Install from Mac App Store or:
   xcode-select --install
   ```
3. **iOS Simulator** (comes with Xcode)
4. **CocoaPods** (if needed)
   ```bash
   brew install cocoapods
   ```

## 🚀 Running on iOS Simulator

### Step 1: Prepare Backend
The backend needs to be accessible from the iOS simulator. You have two options:

#### Option A: Use Docker Compose (Recommended)
```bash
# Start backend on port 8080
docker-compose up -d db api

# Backend will be at http://localhost:8080
```

#### Option B: Use Local Backend
```bash
# Your backend is already running on port 5000
# Continue using it at http://localhost:5000
```

### Step 2: Configure API URL for iOS
Update the API configuration to work with iOS simulator:

```bash
cd frontend

# Create .env file for Expo
echo "EXPO_PUBLIC_API_BASE_URL=http://localhost:8080" > .env

# Or if using local backend on port 5000:
echo "EXPO_PUBLIC_API_BASE_URL=http://localhost:5000" > .env
```

### Step 3: Install Dependencies
```bash
cd frontend
npm install

# Install iOS-specific dependencies if needed
npx pod-install
```

### Step 4: Start iOS App
```bash
# Start Expo with iOS
npm run ios

# Or using Expo directly
npx expo start --ios

# Or start Expo and press 'i' to open iOS
npx expo start
# Then press 'i' in the terminal
```

## 🔧 iOS-Specific Configuration

### Update API Config for iOS
Edit `frontend/src/config/api.ts`:

```typescript
// For iOS Simulator, localhost points to the simulator itself
// Use your Mac's IP or special iOS simulator address

const API_BASE_URL = Platform.select({
  ios: 'http://localhost:8080',  // Works for simulator
  android: 'http://10.0.2.2:8080', // Android emulator
  web: 'http://localhost:8080',
  default: 'http://localhost:8080'
});
```

### Handle iOS Location Permissions
Edit `frontend/app.json` to add iOS permissions:

```json
{
  "expo": {
    "ios": {
      "supportsTablet": true,
      "bundleIdentifier": "com.yourcompany.tindermvp",
      "infoPlist": {
        "NSLocationWhenInUseUsageDescription": "This app uses your location to find potential matches nearby.",
        "NSPhotoLibraryUsageDescription": "This app needs access to photo library to upload profile photos.",
        "NSCameraUsageDescription": "This app needs camera access to take profile photos."
      }
    }
  }
}
```

## 🏃 Running on Physical iPhone

### Step 1: Configure for Physical Device
```bash
# Get your Mac's IP address
ifconfig | grep "inet " | grep -v 127.0.0.1

# Update .env with your Mac's IP
echo "EXPO_PUBLIC_API_BASE_URL=http://YOUR_MAC_IP:8080" > .env
# Example: EXPO_PUBLIC_API_BASE_URL=http://192.168.1.5:8080
```

### Step 2: Connect iPhone
1. Connect iPhone via USB
2. Trust the computer on your iPhone
3. Enable Developer Mode on iPhone (iOS 16+):
   - Settings → Privacy & Security → Developer Mode

### Step 3: Run on Device
```bash
# Using Expo Go app
npx expo start

# Scan QR code with Expo Go app on iPhone
```

### Step 4: Build Native iOS App (Optional)
```bash
# Create development build
npx eas build --platform ios --profile development

# Or local build with Xcode
npx expo prebuild
cd ios
open tinder-expo.xcworkspace
# Then build and run from Xcode
```

## 🐛 Troubleshooting iOS Issues

### Common Problems and Solutions

#### 1. "Network request failed" on iOS Simulator
```bash
# Solution: Use correct URL
# Don't use localhost:5000, use computer's IP or docker-compose default (8080)
```

#### 2. Metro bundler issues
```bash
# Clear cache
npx expo start -c

# Reset Metro
watchman watch-del-all
rm -rf node_modules
npm install
npx expo start --clear
```

#### 3. Pod installation failed
```bash
cd ios
pod deintegrate
pod install
cd ..
```

#### 4. iOS Simulator not opening
```bash
# Open Simulator manually
open -a Simulator

# Then run
npm run ios
```

#### 5. CORS Issues with Backend
Update backend CORS settings in `Program.cs` or `docker-compose.yml`:
```yaml
Cors__AllowedOrigins__3: http://YOUR_MAC_IP:19006
Cors__AllowedOrigins__4: exp://YOUR_MAC_IP:8081
```

## 📋 iOS Testing Checklist

- [ ] App launches on iOS Simulator
- [ ] Can register new account
- [ ] Can login with test credentials
- [ ] Profile photos can be uploaded from library
- [ ] Location permissions requested properly
- [ ] Swipe gestures work smoothly
- [ ] Matches show up correctly
- [ ] Chat messages work in real-time
- [ ] Push notifications configured (if implemented)

## 🔍 Debugging Tips

### View iOS Logs
```bash
# In Expo terminal
# Shake device or press Cmd+D in simulator
# Select "Debug Remote JS"

# Or use React Native Debugger
brew install --cask react-native-debugger
```

### Network Debugging
```bash
# Use Flipper for network inspection
brew install --cask flipper

# Or Charles Proxy
brew install --cask charles
```

## 📱 Device-Specific Settings

### iPhone Notch/SafeArea
The app already uses `react-native-safe-area-context`. Verify it works:
- Top navigation bar doesn't overlap status bar
- Bottom tabs don't overlap home indicator
- Swipe cards respect safe areas

### iOS-Specific Styling
Check these components for iOS appearance:
- Navigation headers
- Tab bar styling
- Modal presentations
- Date picker (iOS native style)

## 🚢 Production Build

### Build for App Store
```bash
# Install EAS CLI
npm install -g eas-cli

# Login to Expo account
eas login

# Configure project
eas build:configure

# Build for iOS
eas build --platform ios --profile production

# Submit to App Store
eas submit --platform ios
```

## 💡 Quick Commands

```bash
# Start iOS development
cd frontend
npm install
npm run ios

# Run on specific simulator
npx expo start --ios --simulator="iPhone 15 Pro"

# Clear everything and restart
watchman watch-del-all
rm -rf node_modules ios/Pods
npm install
cd ios && pod install && cd ..
npm run ios
```

## 🔗 Current Setup Status

✅ **Ready**:
- Expo project configured for iOS
- Backend accessible from simulator
- Basic iOS configuration in app.json

⚠️ **Needs Configuration**:
- iOS-specific permissions in Info.plist
- Bundle identifier for App Store
- Push notification certificates
- App Store assets (icons, screenshots)

## 📝 Next Steps for iOS

1. Test on iOS Simulator first
2. Configure proper bundle ID
3. Test on physical iPhone
4. Add iOS-specific features (haptics, etc.)
5. Prepare for App Store submission