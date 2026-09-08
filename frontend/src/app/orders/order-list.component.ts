import { DatePipe, DecimalPipe, NgClass } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { OrderApiService, readApiError } from './order-api.service';
import { Order, OrderStatus, STATUS_OPTIONS } from './order.model';

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [DatePipe, DecimalPipe, NgClass, RouterLink, FormsModule],
  templateUrl: './order-list.component.html',
  styleUrl: './order-list.component.css'
})
export class OrderListComponent implements OnInit {
  private readonly api = inject(OrderApiService);

  orders: Order[] = [];
  statusFilter = '';
  loading = true;
  error = '';
  readonly statuses = STATUS_OPTIONS;

  ngOnInit(): void {
    this.reload();
  }

  get visible(): Order[] {
    if (!this.statusFilter) {
      return this.orders;
    }
    return this.orders.filter((o) => o.status === this.statusFilter);
  }

  reload(): void {
    this.loading = true;
    this.error = '';
    this.api.list().subscribe({
      next: (orders) => {
        this.orders = orders;
        this.loading = false;
      },
      error: (err) => {
        this.error = readApiError(err);
        this.loading = false;
      }
    });
  }

  statusClass(status: OrderStatus): string {
    return 'st-' + status.toLowerCase();
  }
}
