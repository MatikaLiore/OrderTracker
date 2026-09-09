import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { CreateOrderPayload, MenuItem, Order, OrderStatus } from './order.model';

export interface SubmitResult {
  order: Order;
  created: boolean;
}

@Injectable({ providedIn: 'root' })
export class OrderApiService {
  private readonly http = inject(HttpClient);

  listMenu(): Observable<MenuItem[]> {
    return this.http.get<MenuItem[]>('/api/menu');
  }

  list(): Observable<Order[]> {
    return this.http.get<Order[]>('/api/orders');
  }

  get(id: string): Observable<Order> {
    return this.http.get<Order>(`/api/orders/${id}`);
  }

  submit(payload: CreateOrderPayload): Observable<SubmitResult> {
    return this.http.post<Order>('/api/orders', payload, { observe: 'response' }).pipe(
      map((res) => ({
        order: res.body as Order,
        created: res.status === 201
      }))
    );
  }

  changeStatus(id: string, status: OrderStatus): Observable<Order> {
    return this.http.post<Order>(`/api/orders/${id}/status`, { status });
  }
}
