import { apiClient } from '../config/api';
import { FeedProfile } from '../types';
import { useFeedStore } from '../store/feedStore';

export class FeedService {
  static async getCandidates(
    radiusKm: number = 50,
    page: number = 1,
    pageSize: number = 20
  ): Promise<FeedProfile[]> {
    const response = await apiClient.get<FeedProfile[]>('/feed', {
      params: {
        radiusKm,
        page,
        pageSize,
      },
    });
    return response.data;
  }

  static async loadFeed(
    radiusKm: number = 50,
    page: number = 1
  ): Promise<FeedProfile[]> {
    try {
      const profiles = await this.getCandidates(radiusKm, page, 20);

      // Update store
      if (page === 1) {
        useFeedStore.getState().setProfiles(profiles);
      } else {
        useFeedStore.getState().addProfiles(profiles);
      }

      // Update hasMore flag
      useFeedStore.getState().setHasMore(profiles.length === 20);

      return profiles;
    } catch (error: any) {
      console.error('Failed to load feed:', error);
      throw error;
    }
  }

  static async refreshFeed(): Promise<FeedProfile[]> {
    useFeedStore.getState().resetFeed();
    useFeedStore.getState().setLoading(true);

    try {
      const profiles = await this.loadFeed(50, 1);
      return profiles;
    } finally {
      useFeedStore.getState().setLoading(false);
    }
  }

  static async loadMoreProfiles(): Promise<FeedProfile[]> {
    const { currentPage, isLoading } = useFeedStore.getState();

    if (isLoading) return [];

    useFeedStore.getState().setLoading(true);

    try {
      const nextPage = currentPage + 1;
      const profiles = await this.loadFeed(50, nextPage);
      useFeedStore.getState().incrementPage();
      return profiles;
    } finally {
      useFeedStore.getState().setLoading(false);
    }
  }
}