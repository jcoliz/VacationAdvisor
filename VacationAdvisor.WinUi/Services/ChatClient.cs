using Azure;
using Azure.Identity;
using Azure.AI.Agents.Persistent;
using System.Threading.Tasks;
using VacationAdvisor.WinUi.Options;
using Azure.Core;
using Microsoft.Extensions.Options;
using System;
using System.IO;

namespace VacationAdvisor.WinUi.Services;

// https://learn.microsoft.com/en-us/dotnet/api/overview/azure/ai.agents.persistent-readme?view=azure-dotnet

/// <summary>
///  Client for interacting with a Persistent Agent in Azure AI Foundry.
///  This client allows you to create threads, send messages, and retrieve file content
/// </summary>
/// <param name="credential">TokenCredential for authentication</param>
/// <param name="options">Options for configuring the client, including endpoint and agent ID</param>
public class ChatClient(IOptions<AiFoundryOptions> options, TokenCredential credential)
{
    // Fields
    private readonly PersistentAgentsClient _agentClient = new(options.Value.Endpoint, credential);
    private readonly string _agentId = options.Value.AgentId;

    /// <summary>
    ///  Creates a new thread for the agent.
    ///  A thread is a conversation with the agent where messages can be sent and received.
    /// </summary>
    public async Task<PersistentAgentThread> CreateThreadAsync()
    {
        return await _agentClient.Threads.CreateThreadAsync();
    }

    /// <summary>
    /// Sends a message to the agent in a specific thread and waits for the agent's response.
    /// </summary>
    /// <param name="thread">Thread containing the chat to send into</param>
    /// <param name="content">Message to send to agent</param>
    /// <returns>
    /// Messages in the thread, including the agent's response.
    /// </returns>
    public async Task<AsyncPageable<PersistentThreadMessage>> SendMessageAsync(
        PersistentAgentThread thread,
        string content)
    {
        await _agentClient.Messages.CreateMessageAsync(
            thread.Id,
            MessageRole.User,
            content);

        var agent = await GetAgentAsync();

        ThreadRun run = await _agentClient.Runs.CreateRunAsync(
            thread.Id,
            agent.Id
        );

        await WaitForRunCompletionAsync(thread.Id, run.Id);

        return _agentClient.Messages.GetMessagesAsync(
            threadId: thread.Id, order: ListSortOrder.Ascending);
    }

    /// <summary>
    /// Waits for the agent run to complete, polling status and displaying progress.
    /// </summary>
    private async Task WaitForRunCompletionAsync(string threadId, string runId)
    {
        var startedAt = DateTime.UtcNow;
        ThreadRun run;
        do
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            run = await _agentClient.Runs.GetRunAsync(threadId, runId);

            var elapsed = DateTime.UtcNow - startedAt;
            Console.WriteLine("{0} Run Status: {1}", elapsed, run.Status);
        }
        while (run.Status == RunStatus.Queued
            || run.Status == RunStatus.InProgress);
    }

    /// <summary>
    ///  Displays a message from the thread in the console.
    ///  This method formats the message content and handles different types of content,
    /// </summary>
    /// <param name="threadMessage"></param>
    /// <returns></returns>
    public async Task DisplayMessageAsync(
        PersistentThreadMessage threadMessage)
    {
        Console.Write($"{threadMessage.CreatedAt:yyyy-MM-dd HH:mm:ss} - {threadMessage.Role,10}: ");
        foreach (MessageContent contentItem in threadMessage.ContentItems)
        {
            if (contentItem is MessageTextContent textItem)
            {
                Console.Write(textItem.Text);
            }
            else if (contentItem is MessageImageFileContent imageFileItem)
            {
                await DisplayImageContentAsync(imageFileItem.FileId);
            }
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Downloads and displays image file content.
    /// </summary>
    private async Task DisplayImageContentAsync(string fileId)
    {
        Console.Write($"<image from ID: ./images/{fileId}.png");
        var result = await GetFileContentAsync(fileId);
        var stream = result.ToStream();
        Directory.CreateDirectory("images");
        File.Delete($"images/{fileId}.png");
        using (var fileStream = File.Create($"images/{fileId}.png"))
        {
            await stream.CopyToAsync(fileStream);
        }
    }

    // Private methods
    private async Task<PersistentAgent> GetAgentAsync()
    {
        return await _agentClient.Administration.GetAgentAsync(_agentId);
    }

    private async Task<BinaryData> GetFileContentAsync(
        string fileId)
    {
        return await _agentClient.Files.GetFileContentAsync(fileId);
    }
}