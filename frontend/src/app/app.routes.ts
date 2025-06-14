import { Routes } from '@angular/router';
import { EmployeeList } from './components/employee-list/employee-list';
import { EmployeeDetail } from './components/employee-detail/employee-detail';

export const routes: Routes = [
    { path: '', component: EmployeeList },
    { path: 'employee/:id', component: EmployeeDetail },
    { path: '**', redirectTo: '' } // Redirect any unknown paths to the employee list
];
