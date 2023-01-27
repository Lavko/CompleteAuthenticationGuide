import { Component } from '@angular/core';
import { AuthTestingClient } from '../../services/auth-testing-client';

@Component({
  selector: 'app-home-page',
  templateUrl: './home-page.component.html',
  styleUrls: ['./home-page.component.scss'],
})
export class HomePageComponent {
  public textForAll = '';
  public textForRegistered = '';
  public textForAdmins = '';

  constructor(private authTestingClient: AuthTestingClient) {}

  public getForAll() {
    this.authTestingClient
      .getTextForAll()
      .subscribe(text => (this.textForAll = text));
  }

  public getForRegistered() {
    this.authTestingClient.getTextForRegistered().subscribe(text => {
      this.textForRegistered = text;
    });
  }

  public getForAdmins() {
    this.authTestingClient
      .getTextForAdmins()
      .subscribe(text => (this.textForAdmins = text));
  }
}
