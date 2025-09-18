import apiClient from '../config/api';
import { SwipeRequest, SwipeResult } from '../types/api';

export class SwipeService {
  static async processSwipe(swipeData: SwipeRequest): Promise<SwipeResult> {
    const response = await apiClient.post<SwipeResult>('/swipes', swipeData);
    return response.data;
  }
}