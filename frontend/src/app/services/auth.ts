import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class Auth {
  private apiUrl = 'http://localhost:5018/api/auth/login';

  constructor(private http: HttpClient, private router: Router) {}

  login(email: string, password: string): Observable<any> {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json'
    });
    const body = { email, password };
    console.log('Sending login request:', { email, password });
    return this.http.post<{ token: string }>(this.apiUrl, body, { headers }).pipe(
      tap(response => {
        console.log('Received JWT:', response.token);
        localStorage.setItem('token', response.token);
        this.logTokenPayload(response.token);
      }),
      catchError(error => {
        console.error('Login error:', error.error || 'Invalid credentials');
        return throwError(() => new Error(error.error || 'Invalid credentials'));
      })
    );
  }

  logout(): void {
    console.log('Logging out, removing JWT from localStorage');
    localStorage.removeItem('token');
    this.router.navigate(['/login']);
  }

  isAuthenticated(): boolean {
    const isAuth = !!localStorage.getItem('token');
    console.log('Checking authentication:', isAuth);
    return isAuth;
  }

  getAuthHeader(): { [header: string]: string | string[] } {
    const token = localStorage.getItem('token');
    if (token) {
      const headers = { Authorization: `Bearer ${token}` };
      console.log('Auth headers:', headers);
      return headers;
    } else {
      console.log('Auth headers: {}');
      return {};
    }
  }

  private logTokenPayload(token: string): void {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      console.log('Decoded JWT payload:', payload);
    } catch (e) {
      console.error('Failed to decode JWT:', e);
    }
  }
}
