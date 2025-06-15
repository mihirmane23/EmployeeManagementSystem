import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EmployeeService } from '../../services/employee';
import { Employee } from '../../models/employee';
import { Router, RouterModule } from '@angular/router';
import { ApiResponse } from '../../models/api-response';
import { FormsModule } from '@angular/forms';
import { Auth } from '../../services/auth';

@Component({
  selector: 'app-employee-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './employee-list.html',
  styleUrl: './employee-list.css'
})
export class EmployeeList implements OnInit {
  employees: Employee[] = [];
  errorMessage: string = '';
  currentPage = 1;
  perPage = 10;
  totalEntries = 0;

  showFilter = false;
  filter = {
    name: '',
    department: '',
    salary: null as number | null,
    dateRange: '',
    customStart: '',
    customEnd: ''
  };

  constructor(private employeeService: EmployeeService, private authService: Auth, private router: Router) { }

  ngOnInit(): void {
    this.fetchData();
  }

  toggleFilter(): void {
    this.showFilter = !this.showFilter;
  }

  onDateRangeChange(): void {
    if (this.filter.dateRange !== 'custom') {
      this.filter.customStart = '';
      this.filter.customEnd = '';
    }
  }

  applyFilter(): void {
    this.currentPage = 1;
    this.fetchData();
  }

  resetFilter(): void {
    this.filter = {
      name: '',
      department: '',
      salary: null,
      dateRange: '',
      customStart: '',
      customEnd: ''
    };
    this.currentPage = 1;
    this.fetchData();
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  fetchData(): void {
    this.employeeService.getEmployees(this.currentPage, this.perPage, this.filter).subscribe({
      next: (response: ApiResponse) => {
        this.employees = response.data;
        this.totalEntries = response.totalEntries;
      },
      error: (err) => {
        this.errorMessage = err.message || 'An unknown error occurred!';
      }
    });
  }

  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.fetchData();
    }
  }

  get totalPages(): number {
    return Math.ceil(this.totalEntries / this.perPage);
  }

  get paginationRange(): (number | string)[] {
    const range: (number | string)[] = [];
    const totalPages = this.totalPages;
    const currentPage = this.currentPage;
    const maxVisible = 5; // Changed from 7 to 5

    if (totalPages <= maxVisible) {
      // Show all pages if total is <= 5
      for (let i = 1; i <= totalPages; i++) {
        range.push(i);
      }
    } else {
      // Calculate start and end for 5 pages
      let start = Math.max(1, currentPage - 2);
      let end = Math.min(totalPages, start + maxVisible - 1);

      // Adjust start if end is at totalPages
      if (end === totalPages) {
        start = Math.max(1, end - maxVisible + 1);
      }

      // Add first page and ellipsis if needed
      if (start > 1) {
        range.push(1);
        if (start > 2) {
          range.push('...');
        }
      }

      // Add middle pages
      for (let i = start; i <= end; i++) {
        range.push(i);
      }

      // Add last page and ellipsis if needed
      if (end < totalPages) {
        if (end < totalPages - 1) {
          range.push('...');
        }
        range.push(totalPages);
      }
    }

    return range;
  }
}
