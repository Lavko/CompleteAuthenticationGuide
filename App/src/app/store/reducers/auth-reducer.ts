import { AuthState } from './index';
import { Action, createReducer, on } from '@ngrx/store';
import { AuthActions } from '../actions/actions';
import { Token } from '../../models/token';

export const initialState: AuthState = {
  token: {} as Token,
};

const _authReducer = createReducer(
  initialState,
  on(AuthActions.loggedIn, (state, { token }) => ({ ...state, token: token })),
  on(AuthActions.logOut, () => initialState)
);

export function authReducer(state: any, action: Action) {
  return _authReducer(state, action);
}
