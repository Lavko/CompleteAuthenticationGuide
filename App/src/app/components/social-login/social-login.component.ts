import { Component } from '@angular/core';
import {
  FacebookLoginProvider,
  SocialAuthService,
} from '@abacritt/angularx-social-login';
import { AuthService } from '../../services/auth-service';

@Component({
  selector: 'app-social-login',
  templateUrl: './social-login.component.html',
  styleUrls: ['./social-login.component.css'],
})
export class SocialLoginComponent {
  constructor(
    private socialAuthService: SocialAuthService,
    private authService: AuthService
  ) {}

  signInWithFB(): void {
    this.socialAuthService
      .signIn(FacebookLoginProvider.PROVIDER_ID)
      .then(user => this.authService.socialLogin(user));
  }
}
