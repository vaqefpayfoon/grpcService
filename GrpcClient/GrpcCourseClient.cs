using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using SeedACloud.Grpc;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SeedACloud.GrpcClient
{
    public class GrpcCourseClient
    {

        static readonly GrpcChannel channel;
        static readonly CourseService.CourseServiceClient client;
        static GrpcCourseClient()
        {
            channel = GrpcChannel.ForAddress("https://localhost:5001", new GrpcChannelOptions { });
            client = new CourseService.CourseServiceClient(channel);

        }

        public static async Task GetCourses()  //
        {

            //THIS CAN BE USED TO TEST THE GRPC-WEB PROTOCOL
            //USING THIS CONSOLE CLIENT
            #region CODE
            //Console.WriteLine("Using GrpcWebHandler . . . .");
            //var webChannel = GrpcChannel.ForAddress("https://localhost:5001", new GrpcChannelOptions
            //{
            //    HttpHandler = new GrpcWebHandler(new HttpClientHandler())
            //});

            //var webClient = new CourseService.CourseServiceClient(webChannel);


            //var courseList = await webClient.GetCoursesAsync(new Empty()); 
            #endregion


            var courseList = await client.GetCoursesAsync(new Empty());
                Console.ForegroundColor = ConsoleColor.Green;
                courseList.Courses.ToList().ForEach(c => Console.WriteLine($"Id:{c.Id}Title:{c.Title}$:{c.PaymentType}FirstSection:{c.Sections.FirstOrDefault()?.Title}"));




        }



        public static async Task GetCourseStream()
        {

            //AsyncServerStreamingCall<Course> _streamCall;

            ///var channel = GrpcChannel.ForAddress("https://localhost:5001", new GrpcChannelOptions {});
            /// var client = new CourseService.CourseServiceClient(channel);
            /// 


            var metadata = new Metadata
            {
                { "ClientCode", "SeedACloud_2021" }
            };

            //CANCELLATION TOKEN

            try
            {
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));
                //Only Cancellation Token
                //var response = client.GetCoursesAsStream(new Empty(), cancellationToken: cts.Token).ResponseStream;

                //With metadata and Cancellationtoken
                var response = client.GetCoursesAsStream(new Empty(), metadata, cancellationToken: cts.Token).ResponseStream;


                //DEADLINE
                //var response =  client.GetCoursesAsStream(new Empty(), deadline: DateTime.UtcNow.AddSeconds(12)).ResponseStream;

                //USING AsyncServerStreamingCall
                ///using var _streamCall = client.GetCoursesAsStream(new Empty());
                ///var response = _streamCall.ResponseStream;


                //NORMAL
                //var response = client.GetCoursesAsStream(new Empty()).ResponseStream;
                //var response = client.GetCoursesAsStream(new Empty());
                //var responseStream = response.ResponseStream;

                var courseList = new CourseList();
                ///int ctr = 0;


                while (await response.MoveNext())
                {

                    courseList.Courses.Add(response.Current);
                    Console.WriteLine(response.Current.Id);

                }
            }
            catch (RpcException rpcEx) when (rpcEx.StatusCode == StatusCode.Cancelled)
            {
                Console.WriteLine($"Stream cancelled : {rpcEx.Message}");
            }
            catch (RpcException rpcEx) when (rpcEx.StatusCode == StatusCode.DeadlineExceeded)
            {
                Console.WriteLine($"Deadline exceeded : {rpcEx.Message}");
            }




        }



        public static async Task GetCourseStreamByFilter()
        {

            //GRPC-WEB Handler/Channel
            //Console.WriteLine("Using GrpcWebHandler . . . .");
            //var webChannel = GrpcChannel.ForAddress("https://localhost:5001", new GrpcChannelOptions
            //{
            //    HttpHandler = new GrpcWebHandler(GrpcWebMode.GrpcWebText, new HttpClientHandler())
            //});

            //var webClient = new CourseService.CourseServiceClient(webChannel);

            var filter = new CourseFilter();
            //filter.Level = "Beginner";
            filter.Instructor = "SeedACloud";

            //FOR WEB CLIENT
            //var response = webClient.GetCoursesByFilterAsStream(filter).ResponseStream;


            var response = client.GetCoursesByFilterAsStream(filter).ResponseStream;

            var courses = new CourseList();


            while (await response.MoveNext())
            {
                courses.Courses.Add(response.Current);

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($" Id : {response.Current.Id} ," +
                    $" Title : {response.Current.Title} ," +
                    $" Level : {response.Current.Level} , " +
                    $"Instructor : {response.Current.Instructor} ," +
                    $" PaymentType : {response.Current.PaymentType} ");

            }



        }

        public static async Task SendCourseStream()
        {
            var channel = GrpcChannel.ForAddress("https://localhost:5001", new GrpcChannelOptions { });
            var client = new CourseService.CourseServiceClient(channel);


            Course course = new Course
            {
                Id = 23,
                Title = "FMSA2021",
                Instructor = "SeedACloud",
                PaymentType = Course.Types.PaymentType.Paid,
                Duration = 320
            };

            Course.Types.Section section = new Course.Types.Section
            {
                CourseId = 23,
                Id = 90,
                Title = "FMSA2021-SECTION1",
                SeqNo = 1
            };


            Course.Types.Section.Types.Lecture lecture = new Course.Types.Section.Types.Lecture
            {
                Id = 101,
                SectionId = 90,
                CourseId = 23,
                Name = "FMSA2021-SUBJECT1",
                Subject = new Course.Types.Section.Types.Lecture.Types.Subject() { MediaUrl = "https://cdn.seedacloud.net/9023101" },
                LectureType = "Subject"

            };

            Course.Types.Section.Types.Lecture lecture2 = new Course.Types.Section.Types.Lecture
            {
                Id = 102,
                SectionId = 90,
                CourseId = 23,
                Name = "FMSA2021-ASSIGNMENT1",
                Assignment = new Course.Types.Section.Types.Lecture.Types.Assignment() { Instructions = "Instructions102", Questions = "Questions102" },
                LectureType = "Assignment"
            };

            section.Lectures.Add(lecture);
            section.Lectures.Add(lecture2);

            course.Sections.Add(section);


            Course course2 = new Course
            {
                Id = 24,
                Title = "FMSA2022",
                Instructor = "SeedACloud",
                PaymentType = Course.Types.PaymentType.Paid,
                Duration = 320
            };


            Course course3 = new Course
            {
                Id = 25,
                Title = "FMSA2023",
                Instructor = "SeedACloud",
                PaymentType = Course.Types.PaymentType.Paid,
                Duration = 320
            };

            var courseList = new CourseList();
            courseList.Courses.Add(course);
            courseList.Courses.Add(course2);
            courseList.Courses.Add(course3);




            using (var clientcall = client.SendCourseAsStream())
            {

                foreach (var item in courseList.Courses)
                {
                    await clientcall.RequestStream.WriteAsync(item);
                    //await Task.Delay(2000);
                }



                await clientcall.RequestStream.CompleteAsync();

                var response = await clientcall;
                Console.WriteLine($"Count: {response.Message}");
            }
        }


        public static async Task BiStream()
        {
            var channel = GrpcChannel.ForAddress("https://localhost:5001", new GrpcChannelOptions { });
            var client = new CourseService.CourseServiceClient(channel);

            Course course = new Course
            {
                Id = 26,
                Title = "FMSA2024",
                Instructor = "SeedACloud",
                PaymentType = Course.Types.PaymentType.Paid,
                Duration = 390
            };

            Course course2 = new Course
            {
                Id = 27,
                Title = "FMSA2025",
                Instructor = "SeedACloud",
                PaymentType = Course.Types.PaymentType.Paid,
                Duration = 350
            };


            Course course3 = new Course
            {
                Id = 28,
                Title = "FMSA2026",
                Instructor = "SeedACloud",
                PaymentType = Course.Types.PaymentType.Paid,
                Duration = 329
            };

            var courseList = new CourseList();
            courseList.Courses.Add(course);
            courseList.Courses.Add(course2);
            courseList.Courses.Add(course3);




            // AsyncDuplexStreamingCall<Course, CourseId> none;
            try
            {

                using (var call = client.BiStreamCall())


                {


                    foreach (var item in courseList.Courses)
                    {
                        await call.RequestStream.WriteAsync(item);
                        ///await Task.Delay(2000);
                    }

                    //Console.WriteLine("Disconnecting");
                    await call.RequestStream.CompleteAsync();
                    //await readTask;

                    await foreach (var response in call.ResponseStream.ReadAllAsync())
                    {
                        //print the Ids to the console
                        Console.WriteLine(response.Id);

                    }

                }
            }

            catch (RpcException e)
            {
                // print the gRPC error message
                Console.WriteLine(e.Message);
                //details
                Console.WriteLine(e.Status.Detail);
                //  status code, 
                Console.WriteLine(e.Status.StatusCode);
                //additional details to debug
                Console.WriteLine(e.Status.DebugException);


            }

        }




        public static async Task UpdateCourse()
        {
            Course course = new()
            {
                Id = 27,
                Title = "COMPREHENSIVE GUIDE TO GRPC"
            };

            var response = await client.UpdateCourseAsync(course);



            //Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(response.Message);

        }


        public static async Task DeleteCourse()
        {

            var courseId = new CourseId
            {
                Id = "28"
            };

            var response = await client.DeleteCourseAsync(courseId);

            //Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(response.Message);

        }

    }
}
