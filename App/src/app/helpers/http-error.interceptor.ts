import { Injectable } from '@angular/core';
import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
} from '@angular/common/http';
import {
  BehaviorSubject,
  catchError,
  filter,
  Observable,
  switchMap,
  take,
  throwError,
} from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AuthService } from '../services/auth-service';

@Injectable()
export class HttpErrorInterceptor implements HttpInterceptor {
  private isRefreshing = false;
  constructor(
    private snackbar: MatSnackBar,
    private authService: AuthService
  ) {}

  private refreshTokenSubject: BehaviorSubject<any> = new BehaviorSubject<any>(
    null
  );

  intercept(
    request: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 500) {
          this.snackbar.open(error.error, 'close');
        } else if (error.status === 403) {
          this.snackbar.open('You are not authorized.', 'close');
        } else if (error.status === 401) {
          return this.handle401Error(request, next);
        }
        return throwError(() => error);
      })
    );
  }

  private handle401Error(request: HttpRequest<any>, next: HttpHandler) {
    if (!this.isRefreshing) {
      this.isRefreshing = true;
      this.refreshTokenSubject.next(null);

      const refreshAction = this.authService.refreshToken();
      if (refreshAction) {
        return refreshAction.pipe(
          switchMap(result => {
            this.isRefreshing = false;
            this.refreshTokenSubject.next(result.accessToken);
            return next.handle(request);
          })
        );
      } else {
        return throwError(() => new Error("Don't have tokens"));
      }
    } else {
      return this.refreshTokenSubject.pipe(
        filter(token => token != null),
        take(1),
        switchMap(() => {
          return next.handle(request);
        })
      );
    }
  }
}
