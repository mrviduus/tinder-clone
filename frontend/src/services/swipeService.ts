import { apiClient } from '../config/api';
import { SwipeRequest, SwipeResponse } from '../types';
import { useFeedStore } from '../store/feedStore';

export class SwipeService {
  private static readonly MAX_RETRIES = 3;
  private static readonly RETRY_DELAY = 1000; // 1 second

  static async processSwipe(swipeData: SwipeRequest): Promise<SwipeResponse> {
    let lastError: any;

    for (let attempt = 1; attempt <= this.MAX_RETRIES; attempt++) {
      try {
        const response = await apiClient.post<SwipeResponse>('/swipe', swipeData);

        // Remove swiped profile from feed
        useFeedStore.getState().removeCurrentProfile();

        return response.data;
      } catch (error: any) {
        lastError = error;

        // Don't retry on 4xx errors (client errors)
        if (error.response?.status >= 400 && error.response?.status < 500) {
          throw error;
        }

        // If not the last attempt, wait and retry
        if (attempt < this.MAX_RETRIES) {
          console.log(`Swipe attempt ${attempt} failed, retrying in ${this.RETRY_DELAY}ms...`);
          await new Promise(resolve => setTimeout(resolve, this.RETRY_DELAY * attempt));
        }
      }
    }

    // If all retries failed, throw the last error
    console.error('All swipe attempts failed:', lastError);
    throw lastError;
  }

  static async like(targetUserId: string): Promise<SwipeResponse> {
    const request: SwipeRequest = {
      targetUserId,
      direction: 'like',
    };
    return this.processSwipe(request);
  }

  static async pass(targetUserId: string): Promise<SwipeResponse> {
    const request: SwipeRequest = {
      targetUserId,
      direction: 'pass',
    };
    return this.processSwipe(request);
  }

  // Store pending swipes for offline support
  private static pendingSwipes: SwipeRequest[] = [];

  static async queueSwipe(swipeData: SwipeRequest): Promise<void> {
    this.pendingSwipes.push(swipeData);
    // In a real app, persist this to AsyncStorage
  }

  static async processPendingSwipes(): Promise<void> {
    const swipes = [...this.pendingSwipes];
    this.pendingSwipes = [];

    for (const swipe of swipes) {
      try {
        await this.processSwipe(swipe);
      } catch (error) {
        // Re-queue failed swipes
        this.pendingSwipes.push(swipe);
      }
    }
  }
}