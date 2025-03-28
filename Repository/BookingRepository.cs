﻿using Microsoft.EntityFrameworkCore;
using MyBookingsWebApi.Data;
using MyBookingsWebApi.Models;

namespace MyBookingsWebApi.Repository
{
    public interface IBookingRepository
    {
        Task<Booking?> GetByIdAsync(Guid id);
        Task AddAsync(Booking booking);
        Task RemoveAsync(Booking booking);
        Task<List<Booking>> GetAllBookingsByMemberIdAsync(Guid memberId);
    }
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _context;
        public BookingRepository(AppDbContext context) => _context = context;

        public async Task<Booking?> GetByIdAsync(Guid id)
        {
            return await _context.Bookings.FindAsync(id);
        }
        public async Task<List<Booking>> GetAllBookingsByMemberIdAsync(Guid memberId)
        {
            return await _context.Bookings.Where(x => x.MemberId == memberId).ToListAsync();
        }
        public async Task AddAsync(Booking booking)
        {
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(Booking booking)
        {
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
        }
    }
}
