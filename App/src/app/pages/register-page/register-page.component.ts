import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MyErrorStateMatcher } from '../../helpers/error-state-matcher';
import { AuthService } from '../../services/auth-service';
import { passwordValidator } from '../../helpers/password.validator';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-register-page',
  templateUrl: './register-page.component.html',
  styleUrls: ['./register-page.component.css'],
})
export class RegisterPageComponent implements OnInit {
  public registerForm!: FormGroup;
  public matcher = new MyErrorStateMatcher();

  constructor(
    private authService: AuthService,
    private router: Router,
    private snackbar: MatSnackBar
  ) {}

  ngOnInit() {
    this.registerForm = new FormGroup({
      username: new FormControl(''),
      email: new FormControl('', [Validators.required, Validators.email]),
      password: new FormControl('', [Validators.required, passwordValidator()]),
    });
  }

  public onSubmit() {
    this.authService
      .register(
        this.registerForm.get('username')?.value,
        this.registerForm.get('email')?.value,
        this.registerForm.get('password')?.value
      )
      .subscribe(() => {
        this.snackbar.open('User registered.', 'close');
        this.router.navigate(['login']);
      });
  }
}
