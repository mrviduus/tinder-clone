import apiClient from '../config/api';
import { Profile } from '../types/api';

export class FeedService {
  static async getCandidates(
    radiusKm: number = 50,
    page: number = 1,
    pageSize: number = 20
  ): Promise<Profile[]> {
    const response = await apiClient.get<Profile[]>('/feed', {
      params: {
        radiusKm,
        page,
        pageSize,
      },
    });
    return response.data;
  }
}