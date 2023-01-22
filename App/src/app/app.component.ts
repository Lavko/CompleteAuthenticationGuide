import { Component } from '@angular/core';
import { AuthService } from './services/auth-service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent {
  title = 'App';
  isLogged$ = this.authService.isLoggedIn();
  tokenInfo$ = this.authService.getTokenInfo();

  constructor(private authService: AuthService) {}

  public logOut(): void {
    this.authService.logout();
  }
}
