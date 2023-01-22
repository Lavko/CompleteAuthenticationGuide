import { AuthState } from '../reducers';
import { createFeatureSelector, createSelector } from '@ngrx/store';

export const selectAuthInfo = createFeatureSelector<AuthState>('auth');

export const selectAuthToken = createSelector(
  selectAuthInfo,
  (state: AuthState) => state.token
);
