import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { LoginResponseDto } from './dtos/login-response-dto';
import { RefreshTokenResponseDto } from './dtos/refresh-token-response-dto';

@Injectable({
  providedIn: 'root',
})
export class AuthenticationClient {
  constructor(private http: HttpClient) {}

  public login(
    username: string,
    password: string
  ): Observable<LoginResponseDto> {
    return this.http.post<LoginResponseDto>(
      environment.apiUrl + '/auth/login',
      {
        username: username,
        password: password,
      }
    );
  }

  public socialLogin(
    email: string,
    provider: string,
    accessToken: string
  ): Observable<LoginResponseDto> {
    return this.http.post<LoginResponseDto>(
      environment.apiUrl + '/auth/social-login',
      {
        email: email,
        provider: provider,
        accessToken: accessToken,
      }
    );
  }

  public register(username: string, email: string, password: string) {
    return this.http.post(environment.apiUrl + '/auth/register', {
      username: username,
      email: email,
      password: password,
    });
  }

  public refreshToken(
    accessToken: string,
    refreshToken: string
  ): Observable<RefreshTokenResponseDto> {
    return this.http.post<RefreshTokenResponseDto>(
      environment.apiUrl + '/auth/refresh-token',
      {
        accessToken: accessToken,
        refreshToken: refreshToken,
      }
    );
  }
}
