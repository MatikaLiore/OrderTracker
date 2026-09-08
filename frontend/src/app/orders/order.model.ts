export type OrderStatus = 'Pending' | 'Confirmed' | 'Fulfilled' | 'Cancelled';

export interface Customer {
  id: string;
  name: string;
  email: string;
}

export interface LineItem {
  sku: string;
  name: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface Order {
  id: string;
  externalReference: string;
  customer: Customer;
  lineItems: LineItem[];
  currency: string;
  notes?: string | null;
  subtotal: number;
  total: number;
  status: OrderStatus;
  createdAt: string;
  updatedAt: string;
}

export interface CreateOrderPayload {
  externalReference: string;
  customer: { name: string; email: string };
  currency: string;
  notes?: string;
  lineItems: {
    sku: string;
    name: string;
    quantity: number;
    unitPrice: number;
  }[];
}

export const STATUS_OPTIONS: OrderStatus[] = [
  'Pending',
  'Confirmed',
  'Fulfilled',
  'Cancelled'
];

export function nextStatuses(current: OrderStatus): OrderStatus[] {
  switch (current) {
    case 'Pending':
      return ['Confirmed', 'Cancelled'];
    case 'Confirmed':
      return ['Fulfilled', 'Cancelled'];
    default:
      return [];
  }
}
