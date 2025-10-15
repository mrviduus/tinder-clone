import React, { useState, useEffect } from 'react';
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  StyleSheet,
  ScrollView,
  Alert,
  ActivityIndicator,
} from 'react-native';
import { Picker } from '@react-native-picker/picker';
import { ProfileService } from '../../services/profileService';
import { useProfileStore } from '../../store/profileStore';
import { Gender, UpdateProfileRequest } from '../../types';
import { PRIMARY_COLOR, WHITE, DARK_GRAY } from '../../../assets/styles';

interface EditProfileScreenProps {
  navigation: any;
}

const EditProfileScreen: React.FC<EditProfileScreenProps> = ({ navigation }) => {
  const { profile, setProfile } = useProfileStore();
  const [formData, setFormData] = useState<UpdateProfileRequest>({});
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (profile) {
      setFormData({
        displayName: profile.displayName,
        bio: profile.bio || '',
        searchGender: profile.searchGender,
        ageMin: profile.ageMin,
        ageMax: profile.ageMax,
        maxDistanceKm: profile.maxDistanceKm,
      });
    }
  }, [profile]);

  const handleSave = async () => {
    setLoading(true);
    try {
      await ProfileService.updateProfile(formData);

      // Refresh profile
      const updatedProfile = await ProfileService.getProfile();
      setProfile(updatedProfile);

      Alert.alert('Success', 'Profile updated successfully');
      navigation.goBack();
    } catch (error: any) {
      Alert.alert('Error', error.message || 'Failed to update profile');
    } finally {
      setLoading(false);
    }
  };

  if (!profile) {
    return (
      <View style={styles.container}>
        <ActivityIndicator size="large" color={PRIMARY_COLOR} />
      </View>
    );
  }

  return (
    <ScrollView style={styles.container}>
      <View style={styles.form}>
        <Text style={styles.sectionTitle}>Basic Information</Text>

        <Text style={styles.label}>Display Name</Text>
        <TextInput
          style={styles.input}
          value={formData.displayName}
          onChangeText={(text) => setFormData({ ...formData, displayName: text })}
          placeholder="Enter your display name"
          maxLength={100}
        />

        <Text style={styles.label}>Bio</Text>
        <TextInput
          style={[styles.input, styles.textArea]}
          value={formData.bio}
          onChangeText={(text) => setFormData({ ...formData, bio: text })}
          placeholder="Tell us about yourself..."
          multiline
          numberOfLines={4}
          maxLength={500}
        />

        <Text style={styles.sectionTitle}>Search Preferences</Text>

        <Text style={styles.label}>Looking For</Text>
        <View style={styles.pickerContainer}>
          <Picker
            selectedValue={formData.searchGender}
            onValueChange={(value) => setFormData({ ...formData, searchGender: value })}
            style={styles.picker}
          >
            <Picker.Item label="Male" value={Gender.Male} />
            <Picker.Item label="Female" value={Gender.Female} />
            <Picker.Item label="Everyone" value={Gender.Unknown} />
          </Picker>
        </View>

        <Text style={styles.label}>Age Range</Text>
        <View style={styles.row}>
          <TextInput
            style={[styles.input, styles.halfInput]}
            value={formData.ageMin?.toString()}
            onChangeText={(text) => setFormData({ ...formData, ageMin: parseInt(text) || 18 })}
            placeholder="Min"
            keyboardType="numeric"
            maxLength={3}
          />
          <Text style={styles.separator}>to</Text>
          <TextInput
            style={[styles.input, styles.halfInput]}
            value={formData.ageMax?.toString()}
            onChangeText={(text) => setFormData({ ...formData, ageMax: parseInt(text) || 100 })}
            placeholder="Max"
            keyboardType="numeric"
            maxLength={3}
          />
        </View>

        <Text style={styles.label}>Maximum Distance (km)</Text>
        <TextInput
          style={styles.input}
          value={formData.maxDistanceKm?.toString()}
          onChangeText={(text) => setFormData({ ...formData, maxDistanceKm: parseInt(text) || 50 })}
          placeholder="50"
          keyboardType="numeric"
          maxLength={3}
        />

        <TouchableOpacity
          style={[styles.button, loading && styles.buttonDisabled]}
          onPress={handleSave}
          disabled={loading}
        >
          {loading ? (
            <ActivityIndicator color={WHITE} />
          ) : (
            <Text style={styles.buttonText}>Save Changes</Text>
          )}
        </TouchableOpacity>

        <TouchableOpacity
          style={styles.cancelButton}
          onPress={() => navigation.goBack()}
        >
          <Text style={styles.cancelButtonText}>Cancel</Text>
        </TouchableOpacity>
      </View>
    </ScrollView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: WHITE,
  },
  form: {
    padding: 20,
  },
  sectionTitle: {
    fontSize: 20,
    fontWeight: 'bold',
    color: PRIMARY_COLOR,
    marginTop: 20,
    marginBottom: 15,
  },
  label: {
    fontSize: 16,
    color: DARK_GRAY,
    marginBottom: 5,
  },
  input: {
    borderWidth: 1,
    borderColor: DARK_GRAY,
    borderRadius: 8,
    padding: 12,
    marginBottom: 15,
    fontSize: 16,
  },
  textArea: {
    height: 100,
    textAlignVertical: 'top',
  },
  pickerContainer: {
    borderWidth: 1,
    borderColor: DARK_GRAY,
    borderRadius: 8,
    marginBottom: 15,
  },
  picker: {
    height: 50,
  },
  row: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 15,
  },
  halfInput: {
    flex: 1,
    marginBottom: 0,
  },
  separator: {
    marginHorizontal: 10,
    fontSize: 16,
    color: DARK_GRAY,
  },
  button: {
    backgroundColor: PRIMARY_COLOR,
    padding: 15,
    borderRadius: 8,
    alignItems: 'center',
    marginTop: 20,
  },
  buttonDisabled: {
    opacity: 0.6,
  },
  buttonText: {
    color: WHITE,
    fontSize: 18,
    fontWeight: 'bold',
  },
  cancelButton: {
    padding: 15,
    alignItems: 'center',
    marginTop: 10,
  },
  cancelButtonText: {
    color: PRIMARY_COLOR,
    fontSize: 16,
  },
});

export default EditProfileScreen;