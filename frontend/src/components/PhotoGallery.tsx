import React, { useState } from 'react';
import {
  View,
  Text,
  Image,
  TouchableOpacity,
  StyleSheet,
  Alert,
  ActivityIndicator,
  ScrollView,
  Modal,
} from 'react-native';
import * as ImagePicker from 'expo-image-picker';
import { Ionicons } from '@expo/vector-icons';
import { ProfileService } from '../services/profileService';
import { useProfileStore } from '../store/profileStore';
import { PhotoDto } from '../types';
import { PRIMARY_COLOR, WHITE, DARK_GRAY } from '../../assets/styles';

interface PhotoGalleryProps {
  editable?: boolean;
  maxPhotos?: number;
}

const PhotoGallery: React.FC<PhotoGalleryProps> = ({
  editable = true,
  maxPhotos = 6
}) => {
  const { profile, addPhoto, removePhoto, setPrimaryPhoto } = useProfileStore();
  const [uploading, setUploading] = useState(false);
  const [selectedPhoto, setSelectedPhoto] = useState<PhotoDto | null>(null);
  const [modalVisible, setModalVisible] = useState(false);

  const photos = profile?.photos || [];

  const pickImage = async (source: 'camera' | 'library') => {
    // Request permissions
    const permissionResult = source === 'camera'
      ? await ImagePicker.requestCameraPermissionsAsync()
      : await ImagePicker.requestMediaLibraryPermissionsAsync();

    if (!permissionResult.granted) {
      Alert.alert('Permission Required', 'Please grant permission to access photos');
      return;
    }

    // Launch image picker
    const result = source === 'camera'
      ? await ImagePicker.launchCameraAsync({
          allowsEditing: true,
          aspect: [3, 4],
          quality: 0.8,
        })
      : await ImagePicker.launchImageLibraryAsync({
          allowsEditing: true,
          aspect: [3, 4],
          quality: 0.8,
        });

    if (!result.canceled && result.assets[0]) {
      await uploadPhoto(result.assets[0].uri);
    }
  };

  const uploadPhoto = async (uri: string) => {
    setUploading(true);
    try {
      const photo = await ProfileService.uploadPhoto(uri);
      addPhoto(photo);
      Alert.alert('Success', 'Photo uploaded successfully');
    } catch (error: any) {
      Alert.alert('Error', error.message || 'Failed to upload photo');
    } finally {
      setUploading(false);
    }
  };

  const handleDeletePhoto = (photo: PhotoDto) => {
    Alert.alert(
      'Delete Photo',
      'Are you sure you want to delete this photo?',
      [
        { text: 'Cancel', style: 'cancel' },
        {
          text: 'Delete',
          style: 'destructive',
          onPress: async () => {
            try {
              await ProfileService.deletePhoto(photo.id);
              removePhoto(photo.id);
              setModalVisible(false);
              Alert.alert('Success', 'Photo deleted');
            } catch (error: any) {
              Alert.alert('Error', error.message || 'Failed to delete photo');
            }
          }
        }
      ]
    );
  };

  const handleSetPrimary = async (photo: PhotoDto) => {
    if (photo.isPrimary) return;

    try {
      await ProfileService.setPrimaryPhoto(photo.id);
      setPrimaryPhoto(photo.id);
      setModalVisible(false);
      Alert.alert('Success', 'Primary photo updated');
    } catch (error: any) {
      Alert.alert('Error', error.message || 'Failed to set primary photo');
    }
  };

  const showImageOptions = () => {
    Alert.alert(
      'Add Photo',
      'Choose a source',
      [
        { text: 'Camera', onPress: () => pickImage('camera') },
        { text: 'Gallery', onPress: () => pickImage('library') },
        { text: 'Cancel', style: 'cancel' }
      ]
    );
  };

  const openPhotoModal = (photo: PhotoDto) => {
    setSelectedPhoto(photo);
    setModalVisible(true);
  };

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.title}>Photos</Text>
        <Text style={styles.subtitle}>
          {photos.length}/{maxPhotos} photos
        </Text>
      </View>

      <ScrollView horizontal showsHorizontalScrollIndicator={false}>
        <View style={styles.photoGrid}>
          {photos.map((photo) => (
            <TouchableOpacity
              key={photo.id}
              style={styles.photoWrapper}
              onPress={() => editable ? openPhotoModal(photo) : null}
            >
              <Image
                source={{ uri: `data:image/jpeg;base64,${photo.data}` }}
                style={styles.photo}
              />
              {photo.isPrimary && (
                <View style={styles.primaryBadge}>
                  <Ionicons name="star" size={16} color={WHITE} />
                </View>
              )}
            </TouchableOpacity>
          ))}

          {editable && photos.length < maxPhotos && (
            <TouchableOpacity
              style={[styles.photoWrapper, styles.addPhotoButton]}
              onPress={showImageOptions}
              disabled={uploading}
            >
              {uploading ? (
                <ActivityIndicator color={PRIMARY_COLOR} />
              ) : (
                <>
                  <Ionicons name="add-circle-outline" size={40} color={PRIMARY_COLOR} />
                  <Text style={styles.addPhotoText}>Add Photo</Text>
                </>
              )}
            </TouchableOpacity>
          )}
        </View>
      </ScrollView>

      {/* Photo Options Modal */}
      <Modal
        visible={modalVisible}
        transparent
        animationType="slide"
        onRequestClose={() => setModalVisible(false)}
      >
        <View style={styles.modalOverlay}>
          <View style={styles.modalContent}>
            {selectedPhoto && (
              <>
                <Image
                  source={{ uri: `data:image/jpeg;base64,${selectedPhoto.data}` }}
                  style={styles.modalPhoto}
                />
                <View style={styles.modalActions}>
                  {!selectedPhoto.isPrimary && (
                    <TouchableOpacity
                      style={styles.modalButton}
                      onPress={() => handleSetPrimary(selectedPhoto)}
                    >
                      <Ionicons name="star-outline" size={20} color={PRIMARY_COLOR} />
                      <Text style={styles.modalButtonText}>Set as Primary</Text>
                    </TouchableOpacity>
                  )}
                  <TouchableOpacity
                    style={[styles.modalButton, styles.deleteButton]}
                    onPress={() => handleDeletePhoto(selectedPhoto)}
                  >
                    <Ionicons name="trash-outline" size={20} color="#FF3B30" />
                    <Text style={[styles.modalButtonText, styles.deleteText]}>
                      Delete Photo
                    </Text>
                  </TouchableOpacity>
                  <TouchableOpacity
                    style={styles.modalButton}
                    onPress={() => setModalVisible(false)}
                  >
                    <Text style={styles.modalButtonText}>Cancel</Text>
                  </TouchableOpacity>
                </View>
              </>
            )}
          </View>
        </View>
      </Modal>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    marginVertical: 20,
  },
  header: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 15,
    paddingHorizontal: 20,
  },
  title: {
    fontSize: 18,
    fontWeight: 'bold',
    color: DARK_GRAY,
  },
  subtitle: {
    fontSize: 14,
    color: DARK_GRAY,
  },
  photoGrid: {
    flexDirection: 'row',
    paddingHorizontal: 20,
  },
  photoWrapper: {
    width: 120,
    height: 160,
    marginRight: 10,
    borderRadius: 8,
    overflow: 'hidden',
  },
  photo: {
    width: '100%',
    height: '100%',
  },
  primaryBadge: {
    position: 'absolute',
    top: 8,
    right: 8,
    backgroundColor: PRIMARY_COLOR,
    borderRadius: 12,
    padding: 4,
  },
  addPhotoButton: {
    backgroundColor: '#F2F2F2',
    justifyContent: 'center',
    alignItems: 'center',
    borderWidth: 2,
    borderColor: PRIMARY_COLOR,
    borderStyle: 'dashed',
  },
  addPhotoText: {
    marginTop: 5,
    fontSize: 14,
    color: PRIMARY_COLOR,
  },
  modalOverlay: {
    flex: 1,
    backgroundColor: 'rgba(0, 0, 0, 0.5)',
    justifyContent: 'center',
    alignItems: 'center',
  },
  modalContent: {
    backgroundColor: WHITE,
    borderRadius: 12,
    width: '90%',
    maxWidth: 400,
  },
  modalPhoto: {
    width: '100%',
    height: 400,
    borderTopLeftRadius: 12,
    borderTopRightRadius: 12,
  },
  modalActions: {
    padding: 20,
  },
  modalButton: {
    flexDirection: 'row',
    alignItems: 'center',
    padding: 15,
    borderBottomWidth: 1,
    borderBottomColor: '#F2F2F2',
  },
  modalButtonText: {
    marginLeft: 10,
    fontSize: 16,
    color: DARK_GRAY,
  },
  deleteButton: {
    borderBottomColor: '#FFE5E5',
  },
  deleteText: {
    color: '#FF3B30',
  },
});

export default PhotoGallery;