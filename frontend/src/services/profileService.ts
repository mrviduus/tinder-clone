import { apiClient } from '../config/api';
import {
  ProfileDto,
  UpdateProfileRequest,
  UpdateLocationRequest,
  PhotoDto,
} from '../types';

export class ProfileService {
  static async getProfile(): Promise<ProfileDto> {
    const response = await apiClient.get<ProfileDto>('/profile');
    return response.data;
  }

  static async getPublicProfile(userId: string): Promise<ProfileDto> {
    const response = await apiClient.get<ProfileDto>(`/profile/${userId}`);
    return response.data;
  }

  static async updateProfile(data: UpdateProfileRequest): Promise<void> {
    await apiClient.put('/profile', data);
  }

  static async updateLocation(data: UpdateLocationRequest): Promise<void> {
    await apiClient.put('/profile/location', data);
  }

  static async uploadPhoto(imageUri: string): Promise<PhotoDto> {
    const formData = new FormData();

    // Create file blob for upload
    const fileBlob = {
      uri: imageUri,
      type: 'image/jpeg',
      name: 'photo.jpg',
    } as any;

    formData.append('file', fileBlob);

    const response = await apiClient.post<PhotoDto>('/profile/photos', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  }

  static async deletePhoto(photoId: string): Promise<void> {
    await apiClient.delete(`/profile/photos/${photoId}`);
  }

  static async setPrimaryPhoto(photoId: string): Promise<void> {
    await apiClient.put(`/profile/photos/${photoId}/primary`);
  }
}