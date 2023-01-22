import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { Token } from '../../models/token';

export const AuthActions = createActionGroup({
  source: 'auth',
  events: {
    'Logged In': props<{ token: Token }>(),
    'Log Out': emptyProps(),
  },
});
