import React, { useState } from 'react';
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  StyleSheet,
  Alert,
  ActivityIndicator,
  ScrollView,
} from 'react-native';
import { Picker } from '@react-native-picker/picker';
import { AuthService } from '../services/authService';
import { useAuthStore } from '../store/authStore';
import { RegisterRequest, Gender } from '../types/api';
import { PRIMARY_COLOR, WHITE, DARK_GRAY } from '../../assets/styles';

interface RegisterScreenProps {
  navigation: any;
}

const RegisterScreen: React.FC<RegisterScreenProps> = ({ navigation }) => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [displayName, setDisplayName] = useState('');
  const [birthDate, setBirthDate] = useState('');
  const [gender, setGender] = useState<Gender>(Gender.Unknown);
  const [loading, setLoading] = useState(false);
  const setAuth = useAuthStore((state) => state.setAuth);

  const handleRegister = async () => {
    if (!email || !password || !displayName || !birthDate) {
      Alert.alert('Error', 'Please fill in all fields');
      return;
    }

    if (password.length < 8) {
      Alert.alert('Error', 'Password must be at least 8 characters');
      return;
    }

    setLoading(true);
    try {
      const registerData: RegisterRequest = {
        email,
        password,
        displayName,
        birthDate: new Date(birthDate).toISOString(),
        gender,
      };

      await AuthService.register(registerData);

      // Auto-login after registration
      const authResponse = await AuthService.login({ email, password });

      // Store token immediately for subsequent API calls
      if (typeof localStorage !== 'undefined') {
        localStorage.setItem('accessToken', authResponse.accessToken);
        localStorage.setItem('refreshToken', authResponse.refreshToken);
      }

      const user = await AuthService.getCurrentUser();
      setAuth(authResponse, user);

      navigation.replace('Main');
    } catch (error: any) {
      Alert.alert(
        'Registration Failed',
        error.response?.data?.error || 'Please try again'
      );
    } finally {
      setLoading(false);
    }
  };

  const navigateToLogin = () => {
    navigation.navigate('Login');
  };

  return (
    <ScrollView style={styles.container}>
      <Text style={styles.title}>Create Account</Text>

      <View style={styles.form}>
        <TextInput
          style={styles.input}
          placeholder="Display Name"
          value={displayName}
          onChangeText={setDisplayName}
          autoCapitalize="words"
        />

        <TextInput
          style={styles.input}
          placeholder="Email"
          value={email}
          onChangeText={setEmail}
          keyboardType="email-address"
          autoCapitalize="none"
          autoComplete="email"
        />

        <TextInput
          style={styles.input}
          placeholder="Password (min 8 characters)"
          value={password}
          onChangeText={setPassword}
          secureTextEntry
          autoComplete="password"
        />

        <TextInput
          style={styles.input}
          placeholder="Birth Date (YYYY-MM-DD)"
          value={birthDate}
          onChangeText={setBirthDate}
          keyboardType="numeric"
        />

        <View style={styles.pickerContainer}>
          <Text style={styles.pickerLabel}>Gender:</Text>
          <Picker
            selectedValue={gender}
            onValueChange={(itemValue) => setGender(itemValue)}
            style={styles.picker}
          >
            <Picker.Item label="Male" value={Gender.Male} />
            <Picker.Item label="Female" value={Gender.Female} />
            <Picker.Item label="Non-Binary" value={Gender.NonBinary} />
            <Picker.Item label="Prefer not to say" value={Gender.Unknown} />
          </Picker>
        </View>

        <TouchableOpacity
          style={[styles.button, loading && styles.buttonDisabled]}
          onPress={handleRegister}
          disabled={loading}
        >
          {loading ? (
            <ActivityIndicator color={WHITE} />
          ) : (
            <Text style={styles.buttonText}>Create Account</Text>
          )}
        </TouchableOpacity>

        <TouchableOpacity onPress={navigateToLogin}>
          <Text style={styles.linkText}>
            Already have an account? Login
          </Text>
        </TouchableOpacity>
      </View>
    </ScrollView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: WHITE,
    padding: 20,
  },
  title: {
    fontSize: 32,
    fontWeight: 'bold',
    textAlign: 'center',
    marginBottom: 40,
    marginTop: 60,
    color: PRIMARY_COLOR,
  },
  form: {
    width: '100%',
  },
  input: {
    borderWidth: 1,
    borderColor: DARK_GRAY,
    borderRadius: 8,
    padding: 15,
    marginBottom: 15,
    fontSize: 16,
    backgroundColor: WHITE,
  },
  pickerContainer: {
    marginBottom: 15,
  },
  pickerLabel: {
    fontSize: 16,
    marginBottom: 5,
    color: DARK_GRAY,
  },
  picker: {
    borderWidth: 1,
    borderColor: DARK_GRAY,
    borderRadius: 8,
  },
  button: {
    backgroundColor: PRIMARY_COLOR,
    padding: 15,
    borderRadius: 8,
    alignItems: 'center',
    marginTop: 10,
  },
  buttonDisabled: {
    opacity: 0.6,
  },
  buttonText: {
    color: WHITE,
    fontSize: 18,
    fontWeight: 'bold',
  },
  linkText: {
    textAlign: 'center',
    marginTop: 20,
    color: PRIMARY_COLOR,
    fontSize: 16,
  },
});

export default RegisterScreen;