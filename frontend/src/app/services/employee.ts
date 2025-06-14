import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Employee } from '../models/employee';
import { ApiResponse } from '../models/api-response';

@Injectable({
  providedIn: 'root'
})
export class EmployeeService {
  private apiUrl = 'http://localhost:5018/api/employees';

  constructor(private http: HttpClient) { }

  getEmployees(page: number, perPage: number, filter?: any): Observable<ApiResponse> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('perPage', perPage.toString());

    if (filter) {
      if (filter.name) params = params.set('name', filter.name);
      if (filter.department) params = params.set('department', filter.department);
      if (filter.salary) params = params.set('salary', filter.salary);
      if (filter.dateRange) params = params.set('dateRange', filter.dateRange);
      if (filter.dateRange === 'custom') {
        if (filter.customStart) params = params.set('customStart', filter.customStart);
        if (filter.customEnd) params = params.set('customEnd', filter.customEnd);
      }
    }

    return this.http.get<ApiResponse>(this.apiUrl, { params });
  }

  getEmployee(id: number): Observable<Employee> {
    return this.http.get<Employee>(`${this.apiUrl}/${id}`).pipe(catchError(this.handleError));
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An unknown error occured!';
    if (error.status === 404) {
      errorMessage = error.error || 'Employee not found!';
    } else if (error.status === 500) {
      errorMessage = 'Internal server error occured!';
    }
    return throwError(() => new Error(errorMessage));
  }
}