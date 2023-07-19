using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SeedACloud.Grpc
{
     
   public class GrpcCourseService : CourseService.CourseServiceBase
    {
        private readonly ILogger<GrpcCourseService> _logger;
        private readonly CourseDbContext _courseDbContext;
        public GrpcCourseService(ILogger<GrpcCourseService> logger , CourseDbContext courseDbContext)
        {
            _logger = logger;
            _courseDbContext = courseDbContext;


        }

        //Unary RPC
        public override async Task<CourseList> GetCourses(Empty empty, ServerCallContext context)
        {
   
          //try
            //{
            var courses = await _courseDbContext.Course
                   .Include(c => c.Sections) 
                   .ThenInclude(s => s.Lectures)
                   .Include(c => c.Ratings) 
                   .AsNoTracking() 
                   .AsSplitQuery()
                   .ToListAsync();


            //}
            //catch (RpcException rex) { }


            ///* QUERY FOR A SINGLE COURSE   //TEST 
            #region COURSE WITH SINGLE COURSE - ID : 1
            //var courses = await _courseDbContext.Course.Where(c => c.Id == 1)
            //         .Include(c => c.Section)
            //        .ThenInclude(s => s.Lecture)
            //         .AsNoTracking().AsSplitQuery()
            //        .ToListAsync();
            #endregion


            var courseList = new CourseList();
            courseList.Courses.AddRange(courses);
            return await Task.FromResult(courseList);


        }


        //SERVER STREAMING
        public override async Task GetCoursesAsStream(Empty empty, IServerStreamWriter<Course> responseStream, ServerCallContext context)
        {

            var clientCode = context.RequestHeaders.FirstOrDefault(e => e.Key == "clientcode");
            var ctoken = context.CancellationToken;

            var courses =  await _courseDbContext.Course
                    .Include(c => c.Sections)
                   .ThenInclude(s => s.Lectures)
                   .AsNoTracking()
                   .AsSplitQuery()
                   .ToListAsync();

            foreach (var course in courses)
            {
                if (ctoken.IsCancellationRequested) return;
                ///await Task.Delay(1000); //Enable to see the response being delivered with a slight delay (esp  for cancellation tests)
                await responseStream.WriteAsync(course);
            }

        }


        //SERVER STREAMING WITH FILTER
        public override async Task GetCoursesByFilterAsStream(CourseFilter filter, IServerStreamWriter<Course> responseStream, ServerCallContext context)
        {
            var ctoken = context.CancellationToken;

            var id = filter.Id;
            var level = filter.Level;
            var instructor = filter.Instructor;

            var courses = _courseDbContext.Course.Where(c => (id == 0 || c.Id == id) && 
                                                       (string.IsNullOrEmpty(instructor) == true || c.Instructor == instructor) &&
                                                        (string.IsNullOrEmpty(level) == true || c.Level == level));



            foreach (var item in courses)
            {
                if (ctoken.IsCancellationRequested) return;
                await Task.Delay(1000);
                await responseStream.WriteAsync(item);
            }

        }


        //CLIENT STREAMING
        public override async Task<OKResponse> SendCourseAsStream(IAsyncStreamReader<Course> requestStream, ServerCallContext context)
        {

            while (await requestStream.MoveNext())
            {
                var course = requestStream.Current;
                _courseDbContext.Course.Add(course);
                _courseDbContext.SaveChanges();
                //Console.WriteLine(course.Id);
                
            }
         

            return new OKResponse() { Message =$"created courses... successfully"};
        }



        //BIDIRECTIONAL STREAMING
        public override async Task BiStreamCall(IAsyncStreamReader<Course> requestStream, IServerStreamWriter<CourseId> responseStream, ServerCallContext context)
        {
            await foreach (var course in requestStream.ReadAllAsync())
            {
                try
                {

                    if (_courseDbContext.Course.Any(c => c.Id == course.Id))
                        throw new RpcException(new Status(StatusCode.AlreadyExists, "The course record already exists"));

                    _courseDbContext.Course.Add(course);
                    var count = _courseDbContext.SaveChanges();
                    if (count <= 0)
                        throw new RpcException(new Status(StatusCode.Unknown, "Unable to Save record to database"));


                    await responseStream.WriteAsync(new CourseId() { Id = course.Id.ToString() });
                }
                catch (SqliteException dbEx)
                {
                    var metadata = new Metadata
                    {
                        { "ErrorCode", dbEx.SqliteErrorCode.ToString() } ,
                        { "Source", dbEx.Source } ,
                     
                    };

                    throw new RpcException(new Status(StatusCode.Unknown, dbEx.Message),metadata);
                }
            }
        }


        //UNARY - UPDATE
        public override async Task<OKResponse> UpdateCourse(Course course, ServerCallContext context)
        {

            var selectedCourse = _courseDbContext.Course.FirstOrDefault(c => c.Id == course.Id);

            if (selectedCourse==null)
                throw new RpcException(new Status(StatusCode.Unknown, "Unable to find the record in the database"));

            selectedCourse.Title = course.Title;
            
            var count = _courseDbContext.SaveChanges();
                
            return await Task.FromResult(new OKResponse { Message ="Succesfully Updated"});

        }



        //UNARY DELETE
        public override async Task<OKResponse> DeleteCourse(CourseId courseId, ServerCallContext context)
        {

            var selectedCourse = _courseDbContext.Course.FirstOrDefault(c => c.Id == Int32.Parse(courseId.Id));

            _courseDbContext.Course.Remove(selectedCourse);
            _courseDbContext.SaveChanges();

            return await Task.FromResult(new OKResponse { Message = $"Deleted Course with Id {courseId} " });

        }




    }
}
