import { isDevMode } from '@angular/core';
import { ActionReducerMap, MetaReducer } from '@ngrx/store';
import { Token } from '../../models/token';
import { authReducer } from './auth-reducer';

export const authFeatureName = 'auth';

export interface State {
  auth: AuthState;
}

export interface AuthState {
  token: Token;
}

export const reducers: ActionReducerMap<State> = {
  auth: authReducer,
};

export const metaReducers: MetaReducer<State>[] = isDevMode() ? [] : [];
