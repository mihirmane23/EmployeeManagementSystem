import { CanActivate, CanActivateFn, Router } from '@angular/router';
import { Injectable } from '@angular/core';
import { Auth } from '../services/auth';


@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(private authService: Auth, private router: Router) {}

  canActivate(): boolean {
    if (this.authService.isAuthenticated()) {
      return true;
    }
    this.router.navigate(['/login']);
    return false;
  }
}