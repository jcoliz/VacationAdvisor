using Azure.AI.Agents.Persistent;
using System.Threading.Tasks;
using VacationAdvisor.WinUi.Options;
using Azure.Core;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.IO;
using VacationAdvisor.WinUi.Entities;
using System.Collections.Generic;

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
    public async Task<IAsyncEnumerable<ChatMessage>> SendMessageAsync(
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

        var rawMessages = _agentClient.Messages.GetMessagesAsync
        (
            threadId: thread.Id, order: ListSortOrder.Ascending
        );

        return rawMessages.Select(message => new ChatMessage
        {
            CreatedAt = message.CreatedAt,
            Role = message.Role == MessageRole.User ? ChatMessage.RoleType.User : ChatMessage.RoleType.Assistant,
            Contents = message.ContentItems.Select(contentItem => contentItem switch
            {
                MessageTextContent textContent => new ChatMessage.Content
                {
                    Type = ChatMessage.Content.ContentType.Text,
                    Text = textContent.Text
                },
                MessageImageFileContent imageFileContent => new ChatMessage.Content
                {
                    Type = ChatMessage.Content.ContentType.Image,
                    ImageId = imageFileContent.FileId
                },
                _ => new ChatMessage.Content
                {
                    Type = ChatMessage.Content.ContentType.Unsupported
                }
            }).ToList()
        });
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
        ChatMessage threadMessage)
    {
        Console.Write($"{threadMessage.CreatedAt:yyyy-MM-dd HH:mm:ss} - {threadMessage.Role,10}: ");
        foreach (var contentItem in threadMessage.Contents)
        {
            // TODO: Could refactor content item to get rid of type
            // and just check for nulls
            if (contentItem.Type == ChatMessage.Content.ContentType.Text)
            {
                Console.Write(contentItem.Text);
            }
            else if (contentItem.Type == ChatMessage.Content.ContentType.Image)
            {
                await DisplayImageContentAsync(contentItem.ImageId!);
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
        BinaryData result = await _agentClient.Files.GetFileContentAsync(fileId);
        var stream = result.ToStream();
        Directory.CreateDirectory("images");
        File.Delete($"images/{fileId}.png");
        using var fileStream = File.Create($"images/{fileId}.png");
        await stream.CopyToAsync(fileStream);
    }

    // Private methods
    private async Task<PersistentAgent> GetAgentAsync()
    {
        return await _agentClient.Administration.GetAgentAsync(_agentId);
    }

    public async Task<Stream> GetFileContentAsync(string fileId)
    {
        BinaryData binaryData = await _agentClient.Files.GetFileContentAsync(fileId);
        return binaryData.ToStream();
    }
}