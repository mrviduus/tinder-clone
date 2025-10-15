import React, { useEffect, useState } from 'react';
import {
  View,
  Text,
  Modal,
  TouchableOpacity,
  StyleSheet,
  Image,
  Animated,
  Dimensions,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { MatchDto } from '../types';
import { PRIMARY_COLOR, WHITE, BLACK } from '../../assets/styles';

interface MatchNotificationProps {
  match: MatchDto | null;
  visible: boolean;
  onClose: () => void;
  onViewProfile: () => void;
  onSendMessage: () => void;
}

const { width, height } = Dimensions.get('window');

const MatchNotification: React.FC<MatchNotificationProps> = ({
  match,
  visible,
  onClose,
  onViewProfile,
  onSendMessage,
}) => {
  const [scaleAnim] = useState(new Animated.Value(0));
  const [fadeAnim] = useState(new Animated.Value(0));

  useEffect(() => {
    if (visible) {
      // Animate in
      Animated.parallel([
        Animated.spring(scaleAnim, {
          toValue: 1,
          tension: 20,
          friction: 7,
          useNativeDriver: true,
        }),
        Animated.timing(fadeAnim, {
          toValue: 1,
          duration: 300,
          useNativeDriver: true,
        }),
      ]).start();
    } else {
      // Reset animations
      scaleAnim.setValue(0);
      fadeAnim.setValue(0);
    }
  }, [visible]);

  if (!match) return null;

  const handleClose = () => {
    // Animate out
    Animated.parallel([
      Animated.timing(scaleAnim, {
        toValue: 0,
        duration: 200,
        useNativeDriver: true,
      }),
      Animated.timing(fadeAnim, {
        toValue: 0,
        duration: 200,
        useNativeDriver: true,
      }),
    ]).start(() => {
      onClose();
    });
  };

  return (
    <Modal
      transparent
      visible={visible}
      animationType="none"
      onRequestClose={handleClose}
    >
      <View style={styles.overlay}>
        <Animated.View
          style={[
            styles.container,
            {
              opacity: fadeAnim,
              transform: [{ scale: scaleAnim }],
            },
          ]}
        >
          <TouchableOpacity style={styles.closeButton} onPress={handleClose}>
            <Ionicons name="close" size={30} color={WHITE} />
          </TouchableOpacity>

          <Text style={styles.title}>It's a Match! 🎉</Text>

          <Text style={styles.subtitle}>
            You and {match.counterpart?.displayName} liked each other!
          </Text>

          <View style={styles.photosContainer}>
            <View style={styles.photoWrapper}>
              {/* Current user's photo placeholder */}
              <View style={styles.photoPlaceholder}>
                <Ionicons name="person" size={40} color={WHITE} />
              </View>
            </View>

            <View style={styles.heartContainer}>
              <Ionicons name="heart" size={40} color={PRIMARY_COLOR} />
            </View>

            <View style={styles.photoWrapper}>
              {match.counterpart?.photos?.[0] ? (
                <Image
                  source={{ uri: `data:image/jpeg;base64,${match.counterpart.photos[0].data}` }}
                  style={styles.photo}
                />
              ) : (
                <View style={styles.photoPlaceholder}>
                  <Ionicons name="person" size={40} color={WHITE} />
                </View>
              )}
            </View>
          </View>

          <View style={styles.actions}>
            <TouchableOpacity
              style={[styles.button, styles.sendMessageButton]}
              onPress={onSendMessage}
            >
              <Ionicons name="chatbubble-ellipses" size={20} color={WHITE} />
              <Text style={styles.buttonTextPrimary}>Send Message</Text>
            </TouchableOpacity>

            <TouchableOpacity
              style={[styles.button, styles.viewProfileButton]}
              onPress={onViewProfile}
            >
              <Ionicons name="person" size={20} color={PRIMARY_COLOR} />
              <Text style={styles.buttonTextSecondary}>View Profile</Text>
            </TouchableOpacity>

            <TouchableOpacity
              style={styles.keepSwipingButton}
              onPress={handleClose}
            >
              <Text style={styles.keepSwipingText}>Keep Swiping</Text>
            </TouchableOpacity>
          </View>
        </Animated.View>
      </View>
    </Modal>
  );
};

const styles = StyleSheet.create({
  overlay: {
    flex: 1,
    backgroundColor: 'rgba(0, 0, 0, 0.85)',
    justifyContent: 'center',
    alignItems: 'center',
  },
  container: {
    backgroundColor: WHITE,
    borderRadius: 20,
    padding: 30,
    width: width * 0.9,
    maxWidth: 400,
    alignItems: 'center',
  },
  closeButton: {
    position: 'absolute',
    top: 10,
    right: 10,
    width: 40,
    height: 40,
    borderRadius: 20,
    backgroundColor: 'rgba(0, 0, 0, 0.3)',
    justifyContent: 'center',
    alignItems: 'center',
    zIndex: 1,
  },
  title: {
    fontSize: 32,
    fontWeight: 'bold',
    color: PRIMARY_COLOR,
    marginBottom: 10,
  },
  subtitle: {
    fontSize: 16,
    color: BLACK,
    textAlign: 'center',
    marginBottom: 30,
  },
  photosContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 30,
  },
  photoWrapper: {
    width: 100,
    height: 100,
  },
  photo: {
    width: 100,
    height: 100,
    borderRadius: 50,
    borderWidth: 3,
    borderColor: PRIMARY_COLOR,
  },
  photoPlaceholder: {
    width: 100,
    height: 100,
    borderRadius: 50,
    backgroundColor: '#E0E0E0',
    justifyContent: 'center',
    alignItems: 'center',
    borderWidth: 3,
    borderColor: PRIMARY_COLOR,
  },
  heartContainer: {
    marginHorizontal: 20,
  },
  actions: {
    width: '100%',
  },
  button: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    padding: 15,
    borderRadius: 10,
    marginBottom: 10,
  },
  sendMessageButton: {
    backgroundColor: PRIMARY_COLOR,
  },
  viewProfileButton: {
    backgroundColor: WHITE,
    borderWidth: 2,
    borderColor: PRIMARY_COLOR,
  },
  buttonTextPrimary: {
    color: WHITE,
    fontSize: 16,
    fontWeight: 'bold',
    marginLeft: 8,
  },
  buttonTextSecondary: {
    color: PRIMARY_COLOR,
    fontSize: 16,
    fontWeight: 'bold',
    marginLeft: 8,
  },
  keepSwipingButton: {
    padding: 15,
    alignItems: 'center',
  },
  keepSwipingText: {
    color: '#666',
    fontSize: 14,
  },
});

export default MatchNotification;