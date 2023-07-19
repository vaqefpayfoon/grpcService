using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace SeedACloud.GrpcClient
{
    class Program
    {

       
        static async Task Main(string[] args)
        {
            //try
            //{
                await TestGrpcCalls();
            //}
            //catch (RpcException rpcEx)
            //{
            //    Console.WriteLine(rpcEx.Message);
            //}
            //catch (Exception Ex)
            //{
            //    Console.WriteLine(Ex.Message);
            //    Console.Read();
            //}


        }

        private static async Task TestGrpcCalls()
        {
            // Console.WriteLine("Hello GRPC World!");
            // Console.ReadLine();

            Console.WriteLine("Executing Unary Call GetCourses");
            await GrpcCourseClient.GetCourses();


            //Console.WriteLine("Executing ServerSide Streaming Call");
            //await GrpcCourseClient.GetCourseStream();

            //Console.WriteLine("Executing Client Stream Call");
            //await GrpcCourseClient.SendCourseStream();

            //Console.WriteLine("Executing Bi STream Call");
            //await GrpcCourseClient.BiStream();

            //Console.WriteLine("Executing ServerSide Streaming Call With Filter");
            //await GrpcCourseClient.GetCourseStreamByFilter();

            //Console.WriteLine("Executing Update Course");
            //await GrpcCourseClient.UpdateCourse();


            //Console.WriteLine("Executing Delete Course");
            //await GrpcCourseClient.DeleteCourse();


            Console.Read();
        }
    }

  
}
