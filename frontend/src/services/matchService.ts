import { apiClient } from '../config/api';
import { MatchDto } from '../types';
import { useMatchStore } from '../store/matchStore';

export class MatchService {
  static async getMatches(page: number = 1, pageSize: number = 50): Promise<MatchDto[]> {
    try {
      const response = await apiClient.get<MatchDto[]>('/match', {
        params: {
          page,
          pageSize,
        },
      });

      const matches = response.data;

      // Update store
      if (page === 1) {
        useMatchStore.getState().setMatches(matches);
      } else {
        useMatchStore.getState().addMatches(matches);
      }

      // Update hasMore flag
      useMatchStore.getState().setHasMore(matches.length === pageSize);

      return matches;
    } catch (error: any) {
      console.error('Failed to load matches:', error);
      throw error;
    }
  }

  static async loadMatches(page: number = 1): Promise<MatchDto[]> {
    useMatchStore.getState().setLoading(true);

    try {
      const matches = await this.getMatches(page);
      return matches;
    } catch (error: any) {
      useMatchStore.getState().setError(error.message);
      throw error;
    } finally {
      useMatchStore.getState().setLoading(false);
    }
  }

  static async loadMoreMatches(): Promise<MatchDto[]> {
    const { currentPage, isLoading, hasMore } = useMatchStore.getState();

    if (isLoading || !hasMore) return [];

    const nextPage = currentPage + 1;
    const matches = await this.loadMatches(nextPage);
    useMatchStore.getState().incrementPage();

    return matches;
  }

  static async getMatchDetails(matchId: string): Promise<MatchDto | null> {
    try {
      const response = await apiClient.get<MatchDto>(`/match/${matchId}`);
      return response.data;
    } catch (error) {
      console.error('Failed to get match details:', error);
      return null;
    }
  }

  static async unmatch(matchId: string): Promise<boolean> {
    try {
      await apiClient.post(`/match/${matchId}/unmatch`);

      // Remove from store
      useMatchStore.getState().removeMatch(matchId);

      return true;
    } catch (error) {
      console.error('Failed to unmatch:', error);
      return false;
    }
  }

  static async refreshMatches(): Promise<MatchDto[]> {
    useMatchStore.getState().resetMatches();
    return this.loadMatches(1);
  }
}