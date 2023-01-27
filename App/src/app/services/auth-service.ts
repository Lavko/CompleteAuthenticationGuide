import { Injectable } from '@angular/core';
import { AuthenticationClient } from './auth-client';
import { SocialAuthService, SocialUser } from '@abacritt/angularx-social-login';
import { AuthActions } from '../store/actions/actions';
import { select, Store } from '@ngrx/store';
import jwtDecode from 'jwt-decode';
import { Token } from '../models/token';
import { Claims } from './dtos/claims';
import { map, Observable, take, tap } from 'rxjs';
import { selectAuthToken } from '../store/selectors/auth-selectors';
import { State } from '../store/reducers';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private accessTokenKey = 'accessToken';
  private refreshTokenKey = 'refreshToken';

  constructor(
    private authenticationClient: AuthenticationClient,
    private socialAuthService: SocialAuthService,
    private store: Store<State>,
    private router: Router
  ) {
    this.socialAuthService.authState.subscribe(user => {
      if (user && user.provider && user.provider === 'GOOGLE') {
        this.socialLogin(user);
      }
    });
  }

  public login(username: string, password: string) {
    return this.authenticationClient
      .login(username, password)
      .pipe(take(1))
      .subscribe(response => {
        this.handleSuccessLogging(response.token, response.refreshToken);
      });
  }

  public socialLogin(user: SocialUser) {
    const token = user.provider === 'GOOGLE' ? user.idToken : user.authToken;
    return this.authenticationClient
      .socialLogin(user.email, user.provider, token)
      .pipe(take(1))
      .subscribe(response => {
        this.handleSuccessLogging(response.token, response.refreshToken);
      });
  }

  public register(userName: string, email: string, password: string) {
    return this.authenticationClient
      .register(userName, email, password)
      .pipe(take(1));
  }

  public refreshToken() {
    const accessToken = localStorage.getItem(this.accessTokenKey);
    const refreshToken = localStorage.getItem(this.refreshTokenKey);
    if (accessToken && refreshToken) {
      return this.authenticationClient
        .refreshToken(accessToken, refreshToken)
        .pipe(
          take(1),
          tap(response => {
            this.handleSuccessLogging(
              response.accessToken,
              response.refreshToken
            );
          })
        );
    }
    return false;
  }

  public logout() {
    localStorage.removeItem(this.accessTokenKey);
    localStorage.removeItem(this.refreshTokenKey);
    this.socialAuthService.signOut(true);
    this.store.dispatch(AuthActions.logOut());
    this.router.navigate(['']);
  }

  public isLoggedIn(): Observable<boolean> {
    return this.store.pipe(
      select(selectAuthToken),
      map(token => {
        if (
          token === undefined ||
          token.raw === undefined ||
          token.raw.length < 1
        ) {
          const cacheToken = localStorage.getItem(this.accessTokenKey);
          const refreshToken = localStorage.getItem(this.refreshTokenKey);

          if (!cacheToken || !refreshToken) {
            return false;
          }

          this.handleSuccessLogging(cacheToken, refreshToken);
        }
        return true;
      })
    );
  }

  public getRole(): Observable<string> {
    return this.store.select(selectAuthToken).pipe(map(token => token.role));
  }

  public getTokenInfo(): Observable<Token> {
    return this.store.select(selectAuthToken).pipe(map(token => token));
  }

  public getToken(): Observable<string> {
    return this.store.select(selectAuthToken).pipe(map(token => token?.raw));
  }

  private handleSuccessLogging(accessToken: string, refreshToken: string) {
    const decodedToken = jwtDecode<any>(accessToken);
    const token: Token = {
      userName: decodedToken[Claims.NameTokenKey],
      email: decodedToken[Claims.EmailTokenKey],
      role: decodedToken[Claims.RoleTokenKey],
      raw: accessToken,
    };
    this.store.dispatch(AuthActions.loggedIn({ token }));

    localStorage.setItem(this.accessTokenKey, accessToken);
    localStorage.setItem(this.refreshTokenKey, refreshToken);

    this.router.navigate(['']);
  }
}
