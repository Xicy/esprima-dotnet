﻿using System.Runtime.CompilerServices;
using Esprima.Ast;
using static Esprima.EsprimaExceptionHelper;

namespace Esprima.Utils;

public delegate object? NodePropertyValueAccessor(Node node);

public delegate ref readonly NodeList<T> NodePropertyListValueAccessor<T>(Node node) where T : Node?;

partial class JavaScriptTextWriter
{
    public struct WriteContext
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WriteContext From(Node? parentNode, Node node) =>
            new WriteContext(parentNode, node ?? ThrowArgumentNullException<Node>(nameof(node)));

        internal AdditionalDataSlot _additionalDataSlot;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal WriteContext(Node? parentNode, Node node)
        {
            ParentNode = parentNode;
            Node = node;
            _nodePropertyName = null;
            _nodePropertyValueAccessor = null;
            _additionalDataSlot = default;
        }

        public Node? ParentNode { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public Node Node { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        private string? _nodePropertyName;
        public string? NodePropertyName { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _nodePropertyName; }

        private Delegate? _nodePropertyValueAccessor;
        private Delegate NodePropertyAccessor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _nodePropertyValueAccessor ?? ThrowInvalidOperationException<Delegate>("The context has no associated node property.");
        }

        public bool NodePropertyHasListValue
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => NodePropertyAccessor.GetType().IsGenericType;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Type GetNodePropertyListItemType()
        {
            var type = NodePropertyAccessor.GetType();
            return type.IsGenericType
                ? type.GetGenericArguments()[0]
                : ThrowInvalidOperationException<Type>("The context has an associated node property but its value is not a node list.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetNodePropertyValue<T>() =>
            (T) ((NodePropertyValueAccessor) NodePropertyAccessor)(Node)!;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly NodeList<T> GetNodePropertyListValue<T>() where T : Node? =>
            ref ((NodePropertyListValueAccessor<T>) NodePropertyAccessor)(Node);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearNodeProperty()
        {
            _nodePropertyName = null;
            _nodePropertyValueAccessor = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetNodeProperty(string name, NodePropertyValueAccessor valueAccessor)
        {
            _nodePropertyName = name;
            _nodePropertyValueAccessor = valueAccessor;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ChangeNodeProperty(string name, NodePropertyValueAccessor valueAccessor) =>
            SetNodeProperty(name ?? ThrowArgumentNullException<string>(nameof(name)), valueAccessor ?? ThrowArgumentNullException<NodePropertyValueAccessor>(nameof(valueAccessor)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetNodeProperty<T>(string name, NodePropertyListValueAccessor<T> listValueAccessor) where T : Node?
        {
            _nodePropertyName = name;
            _nodePropertyValueAccessor = listValueAccessor;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ChangeNodeProperty<T>(string name, NodePropertyListValueAccessor<T> listValueAccessor) where T : Node? =>
            SetNodeProperty(name ?? ThrowArgumentNullException<string>(nameof(name)), listValueAccessor ?? ThrowArgumentNullException<NodePropertyListValueAccessor<T>>(nameof(listValueAccessor)));

        /// <summary>
        /// Gets or sets the arbitrary, user-defined data object associated with the current <see cref="WriteContext"/>.
        /// </summary>
        /// <remarks>
        /// The operation is not guaranteed to be thread-safe. In case concurrent access or update is possible, the necessary synchronization is caller's responsibility.
        /// </remarks>
        public object? AssociatedData
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _additionalDataSlot[1];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _additionalDataSlot[1] = value;
        }
    }
}
