import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Auth } from '../../services/auth';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  imports: [FormsModule, CommonModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  email: string = '';
  password: string = '';
  error: string = '';

  constructor(private authService: Auth, private router: Router) {}

  login(event: Event) {
    event.preventDefault(); // Prevent default form submission
    this.authService.login(this.email, this.password).subscribe({
      next: () => {
        console.log('Login successful, navigating to /employees');
        this.error = '';
        this.router.navigate(['/employees']);
      },
      error: (err) => {
        console.error('Login failed:', err.message);
        this.error = err.message || 'Invalid credentials';
      }
    });
  }
}
