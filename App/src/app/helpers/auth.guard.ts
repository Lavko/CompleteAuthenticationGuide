import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router } from '@angular/router';
import { AuthService } from '../services/auth-service';
import { combineLatest, map, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AuthGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot): Observable<boolean> {
    return combineLatest([
      this.authService.isLoggedIn(),
      this.authService.getRole(),
    ]).pipe(
      map(([isLogged, role]) => {
        if (isLogged) {
          if (route.data['roles'] && route.data['roles'].indexOf(role) === -1) {
            this.router.navigate(['/']);
            return false;
          }
          return true;
        }
        this.router.navigate(['/login']);
        return false;
      })
    );
  }
}
