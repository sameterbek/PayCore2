﻿using MassTransit;
using MessageBus;
using Microsoft.AspNetCore.Mvc;
using Orders.API.Models;

namespace Orders.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IPublishEndpoint publishEndpoint;

        public OrdersController(IPublishEndpoint publishEndpoint)
        {
            this.publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        public IActionResult CreateOrder(Order order)
        {
            order.OrderState = OrderState.Pending;
            //db'ye order Pending olarak yazılır.
            //ardından event fırlatılır.
            publishEndpoint.Publish(new OrderCreatedEvent
            {
                CustomerId = order.CustomerId,
                OrderId = order.Id,
                OrderItems = order.OrderItems.Select(od => new MessageBus.OrderItem { Price = od.Price, ProductId = od.ProductId, Stock = od.Stock }).ToList()
            });


            return Ok();
        }
    }
}


/*
 * 1. Sipariş Ekleme eventi fırlar. (OrderCreated)
2. Stocks API’si OrderCreated eventini consume eder.
3. Eğer yeterli stok varsa StockReserved event’i fırlar.
4. Eğer yeterli stok yoksa StockNotReserved event’i fırlar

 5.  Payment API’si StockReserved event’ini consume eder.

 6.  Eğer ödeme alabiliyorsa PaymentCompleted event’i fırlar.

1. Eğer ödeme alamıyorsa PaymentFailed event’i fırlar
2. Orders API PaymentCompleted eventini dinler ve işlem kapanır
3. Order API’si StockNotReserved eventini consume eder ve fırlarsa OrderFailed olarak db’de günceller.
4. Stocks API’si PaymentFailed eventini consume eder ve fırlarsa  stock’ları değiştirir.
5. Order API’si PaymentFailed eventini consume eder ve fırlarsa OrderFailed olarak db’de güncellenir.
 */