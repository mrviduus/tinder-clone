import apiClient from '../config/api';
import {
  Profile,
  UpdateProfileRequest,
  UpdateLocationRequest,
  Photo,
} from '../types/api';

export class ProfileService {
  static async getProfile(): Promise<Profile> {
    const response = await apiClient.get<Profile>('/me');
    return response.data;
  }

  static async updateProfile(data: UpdateProfileRequest): Promise<Profile> {
    const response = await apiClient.put<Profile>('/me', data);
    return response.data;
  }

  static async updateLocation(data: UpdateLocationRequest): Promise<void> {
    await apiClient.put('/me/location', data);
  }

  static async uploadPhoto(imageUri: string): Promise<Photo> {
    const formData = new FormData();

    // Create file blob for upload
    const fileBlob = {
      uri: imageUri,
      type: 'image/jpeg',
      name: 'photo.jpg',
    } as any;

    formData.append('file', fileBlob);

    const response = await apiClient.post<Photo>('/me/photos', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  }

  static async deletePhoto(photoId: string): Promise<void> {
    await apiClient.delete(`/me/photos/${photoId}`);
  }
}