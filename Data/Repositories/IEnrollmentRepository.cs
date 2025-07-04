﻿using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Data.Entities.Enums;
using GestaoEscolarWeb.Models;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GestaoEscolarWeb.Data.Repositories
{
    public interface IEnrollmentRepository : IGenericRepository<Enrollment>
    {
        Task<IEnumerable<Enrollment>> GetEnrollmentsWithStudentAndSubjectAsync();

        Task<Enrollment> GetEnrollmentWithStudentAndSubjectByIdAsync(int id);

        Task<bool> ExistingEnrollmentAsync(Student student, CreateEnrollmentViewModel model);

        Task<StudentStatus> GetStudentStatusAsync(Enrollment enrollmentSearch);
    }
}
