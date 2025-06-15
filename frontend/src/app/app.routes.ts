import { Routes } from '@angular/router';
import { EmployeeList } from './components/employee-list/employee-list';
import { EmployeeDetail } from './components/employee-detail/employee-detail';
import { Login } from './components/login/login';
import { AuthGuard } from './guards/auth-guard';

export const routes: Routes = [
    { path: 'login', component: Login },
    { path: '', redirectTo: '/login', pathMatch: 'full' },
    { path: 'employees', component: EmployeeList, canActivate: [AuthGuard] },
    { path: 'employee/:id', component: EmployeeDetail, canActivate: [AuthGuard] },
    { path: '**', redirectTo: '/login', pathMatch: 'full' }  // Redirect any unknown paths to the employee list
];
