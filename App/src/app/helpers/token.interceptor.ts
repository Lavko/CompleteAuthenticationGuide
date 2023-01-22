import {
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { combineLatest, Observable, switchMap } from 'rxjs';
import { AuthService } from '../services/auth-service';

@Injectable()
export class TokenInterceptor implements HttpInterceptor {
  constructor(public authService: AuthService) {}

  intercept(
    request: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    return combineLatest([
      this.authService.isLoggedIn(),
      this.authService.getToken(),
    ]).pipe(
      switchMap(([isLogged, token]) => {
        if (isLogged) {
          const newRequest = request.clone({
            setHeaders: {
              Authorization: `Bearer ${token}`,
            },
          });
          return next.handle(newRequest);
        }
        return next.handle(request);
      })
    );
  }
}
