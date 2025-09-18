import React, { useState, useEffect } from "react";
import {
  ScrollView,
  View,
  Text,
  ImageBackground,
  TouchableOpacity,
  TextInput,
  Alert,
  Image,
  ActivityIndicator,
} from "react-native";
import * as ImagePicker from 'expo-image-picker';
import * as Location from 'expo-location';
import { Icon, ProfileItem } from "../components";
import { useAuthStore } from "../src/store/authStore";
import { ProfileService } from "../src/services/profileService";
import { Profile as ProfileType, UpdateProfileRequest } from "../src/types/api";
import styles, { WHITE, PRIMARY_COLOR, DARK_GRAY } from "../assets/styles";

const Profile = () => {
  const [profile, setProfile] = useState<ProfileType | null>(null);
  const [loading, setLoading] = useState(true);
  const [editing, setEditing] = useState(false);
  const [formData, setFormData] = useState<UpdateProfileRequest>({});
  const { user, clearAuth } = useAuthStore();

  useEffect(() => {
    loadProfile();
  }, []);

  const loadProfile = async () => {
    try {
      const profileData = await ProfileService.getProfile();
      setProfile(profileData);
      setFormData({
        displayName: profileData.displayName,
        bio: profileData.bio || '',
        jobTitle: profileData.jobTitle || '',
        company: profileData.company || '',
        school: profileData.school || '',
      });
    } catch (error) {
      Alert.alert('Error', 'Failed to load profile');
    } finally {
      setLoading(false);
    }
  };

  const handleUpdateProfile = async () => {
    try {
      setLoading(true);
      const updatedProfile = await ProfileService.updateProfile(formData);
      setProfile(updatedProfile);
      setEditing(false);
      Alert.alert('Success', 'Profile updated successfully');
    } catch (error) {
      Alert.alert('Error', 'Failed to update profile');
    } finally {
      setLoading(false);
    }
  };

  const handleImagePicker = async () => {
    const { status } = await ImagePicker.requestMediaLibraryPermissionsAsync();
    if (status !== 'granted') {
      Alert.alert('Permission needed', 'Please allow access to your photos');
      return;
    }

    const result = await ImagePicker.launchImageLibraryAsync({
      mediaTypes: ImagePicker.MediaTypeOptions.Images,
      allowsEditing: true,
      aspect: [1, 1],
      quality: 0.8,
    });

    if (!result.canceled && result.assets[0]) {
      try {
        setLoading(true);
        const imageUri = result.assets[0].uri;
        await ProfileService.uploadPhoto(imageUri);
        await loadProfile();
        Alert.alert('Success', 'Photo uploaded successfully');
      } catch (error) {
        Alert.alert('Error', 'Failed to upload photo');
      } finally {
        setLoading(false);
      }
    }
  };

  const handleLocationUpdate = async () => {
    const { status } = await Location.requestForegroundPermissionsAsync();
    if (status !== 'granted') {
      Alert.alert('Permission needed', 'Please allow access to your location');
      return;
    }

    try {
      setLoading(true);
      const location = await Location.getCurrentPositionAsync({});
      await ProfileService.updateLocation({
        latitude: location.coords.latitude,
        longitude: location.coords.longitude,
      });
      Alert.alert('Success', 'Location updated successfully');
    } catch (error) {
      Alert.alert('Error', 'Failed to update location');
    } finally {
      setLoading(false);
    }
  };

  const handleLogout = () => {
    Alert.alert(
      'Logout',
      'Are you sure you want to logout?',
      [
        { text: 'Cancel', style: 'cancel' },
        { text: 'Logout', style: 'destructive', onPress: clearAuth },
      ]
    );
  };

  if (loading && !profile) {
    return (
      <View style={[styles.container, { justifyContent: 'center', alignItems: 'center' }]}>
        <ActivityIndicator size="large" color={PRIMARY_COLOR} />
      </View>
    );
  }

  const mainPhoto = profile?.photos?.find(p => p.isMain) || profile?.photos?.[0];

  return (
    <ImageBackground
      source={require("../assets/images/bg.png")}
      style={styles.bg}
    >
      <ScrollView style={styles.containerProfile}>
        <View style={styles.photo}>
          {mainPhoto ? (
            <Image
              source={{ uri: `data:image/jpeg;base64,${mainPhoto.photoData}` }}
              style={styles.photo}
            />
          ) : (
            <View style={[styles.photo, { backgroundColor: DARK_GRAY, justifyContent: 'center', alignItems: 'center' }]}>
              <Text style={{ color: WHITE, fontSize: 18 }}>No Photo</Text>
            </View>
          )}

          <View style={styles.top}>
            <TouchableOpacity onPress={handleImagePicker}>
              <Icon
                name="camera"
                size={20}
                color={WHITE}
                style={styles.topIconLeft}
              />
            </TouchableOpacity>

            <TouchableOpacity onPress={handleLogout}>
              <Icon
                name="log-out"
                size={20}
                color={WHITE}
                style={styles.topIconRight}
              />
            </TouchableOpacity>
          </View>
        </View>

        <View style={{ padding: 20, backgroundColor: WHITE }}>
          {editing ? (
            <View>
              <Text style={{ fontSize: 18, fontWeight: 'bold', marginBottom: 15 }}>
                Edit Profile
              </Text>

              <TextInput
                style={styles.input}
                placeholder="Display Name"
                value={formData.displayName}
                onChangeText={(text) => setFormData({ ...formData, displayName: text })}
              />

              <TextInput
                style={[styles.input, { height: 80 }]}
                placeholder="Bio"
                value={formData.bio}
                onChangeText={(text) => setFormData({ ...formData, bio: text })}
                multiline
              />

              <TextInput
                style={styles.input}
                placeholder="Job Title"
                value={formData.jobTitle}
                onChangeText={(text) => setFormData({ ...formData, jobTitle: text })}
              />

              <TextInput
                style={styles.input}
                placeholder="Company"
                value={formData.company}
                onChangeText={(text) => setFormData({ ...formData, company: text })}
              />

              <TextInput
                style={styles.input}
                placeholder="School"
                value={formData.school}
                onChangeText={(text) => setFormData({ ...formData, school: text })}
              />

              <View style={{ flexDirection: 'row', justifyContent: 'space-between', marginTop: 20 }}>
                <TouchableOpacity
                  style={[styles.button, { backgroundColor: DARK_GRAY, flex: 0.45 }]}
                  onPress={() => setEditing(false)}
                >
                  <Text style={styles.buttonText}>Cancel</Text>
                </TouchableOpacity>

                <TouchableOpacity
                  style={[styles.button, { flex: 0.45 }]}
                  onPress={handleUpdateProfile}
                  disabled={loading}
                >
                  {loading ? (
                    <ActivityIndicator color={WHITE} />
                  ) : (
                    <Text style={styles.buttonText}>Save</Text>
                  )}
                </TouchableOpacity>
              </View>
            </View>
          ) : (
            <View>
              <View style={{ flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
                <Text style={{ fontSize: 24, fontWeight: 'bold' }}>
                  {profile?.displayName || user?.displayName}, {profile ? new Date().getFullYear() - new Date(profile.birthDate).getFullYear() : ''}
                </Text>
                <TouchableOpacity onPress={() => setEditing(true)}>
                  <Icon name="pencil" size={20} color={PRIMARY_COLOR} />
                </TouchableOpacity>
              </View>

              {profile?.bio && (
                <View style={{ marginBottom: 15 }}>
                  <Text style={{ fontSize: 16, color: DARK_GRAY }}>{profile.bio}</Text>
                </View>
              )}

              {profile?.jobTitle && (
                <View style={{ marginBottom: 10 }}>
                  <Text style={{ fontSize: 14, color: DARK_GRAY }}>
                    💼 {profile.jobTitle}{profile.company && ` at ${profile.company}`}
                  </Text>
                </View>
              )}

              {profile?.school && (
                <View style={{ marginBottom: 10 }}>
                  <Text style={{ fontSize: 14, color: DARK_GRAY }}>
                    🎓 {profile.school}
                  </Text>
                </View>
              )}

              {/* Photo Management Section */}
              <View style={{ marginTop: 20 }}>
                <Text style={{ fontSize: 18, fontWeight: 'bold', marginBottom: 15 }}>
                  Photos ({profile?.photos?.length || 0})
                </Text>

                <ScrollView horizontal showsHorizontalScrollIndicator={false}>
                  <View style={{ flexDirection: 'row', marginBottom: 15 }}>
                    {profile?.photos?.map((photo, index) => (
                      <View key={photo.photoId} style={{ marginRight: 10 }}>
                        <Image
                          source={{ uri: `data:image/jpeg;base64,${photo.photoData}` }}
                          style={{
                            width: 80,
                            height: 80,
                            borderRadius: 8,
                            borderWidth: photo.isMain ? 3 : 1,
                            borderColor: photo.isMain ? PRIMARY_COLOR : DARK_GRAY
                          }}
                        />
                        {photo.isMain && (
                          <Text style={{
                            fontSize: 10,
                            textAlign: 'center',
                            color: PRIMARY_COLOR,
                            fontWeight: 'bold',
                            marginTop: 2
                          }}>
                            Main
                          </Text>
                        )}
                      </View>
                    ))}

                    {/* Add Photo Button */}
                    <TouchableOpacity
                      onPress={handleImagePicker}
                      style={{
                        width: 80,
                        height: 80,
                        borderRadius: 8,
                        borderWidth: 2,
                        borderStyle: 'dashed',
                        borderColor: PRIMARY_COLOR,
                        justifyContent: 'center',
                        alignItems: 'center',
                        backgroundColor: '#f8f8f8'
                      }}
                      disabled={loading}
                    >
                      {loading ? (
                        <ActivityIndicator color={PRIMARY_COLOR} size="small" />
                      ) : (
                        <>
                          <Icon name="plus" size={24} color={PRIMARY_COLOR} />
                          <Text style={{ fontSize: 10, color: PRIMARY_COLOR, marginTop: 4 }}>
                            Add Photo
                          </Text>
                        </>
                      )}
                    </TouchableOpacity>
                  </View>
                </ScrollView>
              </View>

              <TouchableOpacity
                style={[styles.button, { marginTop: 20 }]}
                onPress={handleLocationUpdate}
                disabled={loading}
              >
                {loading ? (
                  <ActivityIndicator color={WHITE} />
                ) : (
                  <Text style={styles.buttonText}>📍 Update Location</Text>
                )}
              </TouchableOpacity>
            </View>
          )}
        </View>
      </ScrollView>
    </ImageBackground>
  );
};

export default Profile;
