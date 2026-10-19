export interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  pictureUrl: string;
  productTypeId: number;
  productBrandId: number;
  productType: string;
  productBrand: string;
}

export interface ProductPage {
  pageIndex: number;
  pageSize: number;
  count: number;
  data: Product[];
}

export interface LookupItem {
  id: number;
  name: string;
}

export interface BasketItem {
  id: number;
  productName: string;
  price: number;
  quantity: number;
  pictureUrl: string;
  brand: string;
  type: string;
}

export interface Basket {
  id: string;
  items: BasketItem[];
  paymentIntentId?: string;
  clientSecret?: string;
  deliveryMethodId?: number | null;
  shippingPrice?: number;
}

export interface DeliveryMethod {
  id: number;
  shortName: string;
  description: string;
  cost: number;
  deliveryTime: string;
}

export interface UserProfile {
  displayName: string;
  email: string;
  token: string;
}

export interface AuthToken {
  token: string;
  refreshToken: string;
  expiresAt: string;
}

export interface OrderItem {
  productName: string;
  productId: number;
  price: number;
  pictureUrl: string;
  quantity: number;
}

export interface UserOrder {
  id: number;
  orderDate: string;
  orderStatus: string;
  subtotal: number;
  items: OrderItem[];
}

export interface OrdersSummary {
  buyerEmail: string;
  totalOrders: number;
  totalSpent: number;
  orders: UserOrder[];
}

export interface ShippingAddress {
  firstName: string;
  lastName: string;
  street: string;
  city: string;
  state: string;
}

export interface StripeConfig {
  publishableKey: string;
}
